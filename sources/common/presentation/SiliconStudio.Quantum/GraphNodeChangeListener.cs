// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// An object that tracks the changes in the content of <see cref="IContentNode"/> referenced by a given root node.
    /// A <see cref="GraphNodeChangeListener"/> will raise events on changes on any node that is either a child, or the
    /// target of a reference from the root node, recursively.
    /// </summary>
    public class GraphNodeChangeListener : INotifyContentValueChange, IDisposable
    {
        private readonly IContentNode rootNode;
        private readonly Func<IMemberNode, IContentNode, bool> shouldRegisterNode;
        protected readonly HashSet<IContentNode> RegisteredNodes = new HashSet<IContentNode>();

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphNodeChangeListener"/> class.
        /// </summary>
        /// <param name="rootNode">The root node for which to track referenced node changes.</param>
        /// <param name="shouldRegisterNode">A method that can indicate whether a node of the hierarchy should be registered to the listener.</param>
        public GraphNodeChangeListener(IContentNode rootNode, Func<IMemberNode, IContentNode, bool> shouldRegisterNode = null)
        {
            this.rootNode = rootNode;
            this.shouldRegisterNode = shouldRegisterNode;
            RegisterAllNodes();
        }

        /// <summary>
        /// Raised before one of the node referenced by the related root node changes.
        /// </summary>
        public event EventHandler<MemberNodeChangeEventArgs> Changing;

        /// <summary>
        /// Raised after one of the node referenced by the related root node has changed.
        /// </summary>
        public event EventHandler<MemberNodeChangeEventArgs> Changed;

        /// <inheritdoc/>
        public void Dispose()
        {
            var visitor = new GraphVisitorBase();
            visitor.Visiting += (node, path) => UnregisterNode(node);
            visitor.Visit(rootNode);
        }

        protected virtual bool RegisterNode(IContentNode node)
        {
            // A node can be registered multiple times when it is referenced via multiple paths
            var memberNode = node as IMemberNode;
            if (RegisteredNodes.Add(node))
            {
                if (memberNode != null)
                {
                    ((IInitializingMemberNode)memberNode).PrepareChange += ContentPrepareChange;
                    ((IInitializingMemberNode)memberNode).FinalizeChange += ContentFinalizeChange;
                    memberNode.Changing += ContentChanging;
                    memberNode.Changed += ContentChanged;
                    return true;
                }
            }

            return false;
        }

        protected virtual bool UnregisterNode(IContentNode node)
        {
            var memberNode = node as IMemberNode;
            if (RegisteredNodes.Remove(node))
            {
                if (memberNode != null)
                {
                    ((IInitializingMemberNode)memberNode).PrepareChange -= ContentPrepareChange;
                    ((IInitializingMemberNode)memberNode).FinalizeChange -= ContentFinalizeChange;
                    memberNode.Changing -= ContentChanging;
                    memberNode.Changed -= ContentChanged;
                    return true;
                }
            }
            return false;
        }

        private void RegisterAllNodes()
        {
            var visitor = new GraphVisitorBase();
            visitor.Visiting += (node, path) => RegisterNode(node);
            visitor.ShouldVisit = shouldRegisterNode;
            visitor.Visit(rootNode);
        }

        private void ContentPrepareChange(object sender, INodeChangeEventArgs e)
        {
            var node = e.Node;
            var visitor = new GraphVisitorBase();
            visitor.Visiting += (node1, path) => UnregisterNode(node1);
            visitor.ShouldVisit = shouldRegisterNode;
            switch (e.ChangeType)
            {
                case ContentChangeType.ValueChange:
                case ContentChangeType.CollectionUpdate:
                    // The changed node itself is still valid, we don't want to unregister it
                    visitor.SkipRootNode = true;
                    visitor.Visit(node);
                    // TODO: In case of CollectionUpdate we could probably visit only the target node of the corresponding index
                    break;
                case ContentChangeType.CollectionRemove:
                    if (node.IsReference && e.OldValue != null)
                    {
                        var removedNode = node.ItemReferences[e.Index].TargetNode;
                        if (removedNode != null)
                        {
                            visitor.Visit(removedNode, node as MemberContent);
                        }
                    }
                    break;
            }
        }

        private void ContentFinalizeChange(object sender, INodeChangeEventArgs e)
        {
            var visitor = new GraphVisitorBase();
            visitor.Visiting += (node, path) => RegisterNode(node);
            visitor.ShouldVisit = shouldRegisterNode;
            switch (e.ChangeType)
            {
                case ContentChangeType.ValueChange:
                case ContentChangeType.CollectionUpdate:
                    // The changed node itself is still valid, we don't want to re-register it
                    visitor.SkipRootNode = true;
                    visitor.Visit(e.Node);
                    // TODO: In case of CollectionUpdate we could probably visit only the target node of the corresponding index
                    break;
                case ContentChangeType.CollectionAdd:
                    if (e.Node.IsReference && e.NewValue != null)
                    {
                        IContentNode addedNode;
                        Index index;
                        if (!e.Index.IsEmpty)
                        {
                            index = e.Index;
                            addedNode = e.Node.ItemReferences[e.Index].TargetNode;
                        }
                        else
                        {
                            var reference = e.Node.ItemReferences.First(x => x.TargetNode.Retrieve() == e.NewValue);
                            index = reference.Index;
                            addedNode = reference.TargetNode;
                        }

                        if (addedNode != null)
                        {
                            var path = new GraphNodePath(e.Node).PushIndex(index);
                            visitor.Visit(addedNode, e.Node as MemberContent, path);
                        }
                    }
                    break;
            }
        }

        private void ContentChanging(object sender, MemberNodeChangeEventArgs e)
        {
            Changing?.Invoke(sender, e);
        }

        private void ContentChanged(object sender, MemberNodeChangeEventArgs e)
        {
            Changed?.Invoke(sender, e);
        }
    }
}

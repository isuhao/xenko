// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;

using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Shaders.Ast.Xenko
{
    /// <summary>
    /// An enum.
    /// </summary>
    public partial class EnumType : TypeBase, IDeclaration, IScopeContainer
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "EnumType" /> class.
        /// </summary>
        public EnumType()
        {
            Values = new List<Expression>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///   Gets or sets the fields.
        /// </summary>
        /// <value>
        ///   The fields.
        /// </value>
        public List<Expression> Values { get; set; }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public override IEnumerable<Node> Childrens()
        {
            ChildrenList.Clear();
            ChildrenList.Add(Name);
            ChildrenList.AddRange(Values);
            return ChildrenList;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("enum {0} {{...}}", Name);
        }

        /// <inheritdoc/>
        public bool Equals(EnumType other)
        {
            return base.Equals(other);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return Equals(obj as EnumType);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(EnumType left, EnumType right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EnumType left, EnumType right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}

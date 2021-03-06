/*
* sones GraphDB - Community Edition - http://www.sones.com
* Copyright (C) 2007-2011 sones GmbH
*
* This file is part of sones GraphDB Community Edition.
*
* sones GraphDB is free software: you can redistribute it and/or modify
* it under the terms of the GNU Affero General Public License as published by
* the Free Software Foundation, version 3 of the License.
* 
* sones GraphDB is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
* GNU Affero General Public License for more details.
*
* You should have received a copy of the GNU Affero General Public License
* along with sones GraphDB. If not, see <http://www.gnu.org/licenses/>.
* 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.GraphQL.GQL.Structure.Nodes.Misc;
using sones.GraphQL.GQL.Structure.Helper.ExpressionGraph;
using sones.GraphQL.Structure.Helper.Enums;
using sones.GraphDB.TypeSystem;
using sones.GraphQL.GQL.Structure.Nodes.Expressions;
using sones.Plugins.SonesGQL.Aggregates;

namespace sones.GraphQL.GQL.Manager.Select
{

    #region SelectionElement

    /// <summary>
    /// Stores some information about the current selection element. This is one part of a IDCHain.
    /// </summary>
    public class SelectionElement
    {

        #region Properties

        public IDChainDefinition RelatedIDChainDefinition { get; set; }
        public String Alias { get; set; }
        public Int64? TypeID { get; protected set; }
        public EdgeList EdgeList { get; protected set; }
        public LevelKey LevelKey { get; protected set; }
        public Boolean IsGroupedOrAggregated { get; protected set; }
        public TypesOfSelect Selection { get; protected set; }
        public IAttributeDefinition Element { get; set; }

        /// <summary>
        /// This is a kind of a static (or dynamic calculated) value which will be set as value for the selected attribute.
        /// </summary>
        public SelectValueAssignment SelectValueAssignment { get; set; }

        #endregion

        #region Ctors

        public SelectionElement() { }

        public SelectionElement(TypesOfSelect mySelType, Int64? myTypeID = null)
        {
            Selection = mySelType;
            TypeID = myTypeID;
        }

        public SelectionElement(string myAlias, IDChainDefinition myRelatedIDChainDefinition)
        {
            Alias = myAlias;
            RelatedIDChainDefinition = myRelatedIDChainDefinition;
        }

        public SelectionElement(string myAlias, Select.EdgeList myEdgeList, bool myIsGroupedOrAggregated, IDChainDefinition myRelatedIDChainDefinition, IAttributeDefinition myElement = null)
            : this(myAlias, myRelatedIDChainDefinition)
        {
            EdgeList = myEdgeList;
            IsGroupedOrAggregated = myIsGroupedOrAggregated;
            Element = myElement;
        }

        #endregion

        /// <summary>
        /// Checks whether this element is a reference AND has some following selections
        /// </summary>
        /// <returns></returns>
        public Boolean IsReferenceToSkip(EdgeList myEdgeList)
        {
            if (Element != null && !(this is SelectionElementFunction))
            {
                if (Element.Kind == AttributeType.IncomingEdge || Element.Kind == AttributeType.OutgoingEdge)
                {
                    if (RelatedIDChainDefinition.Edges.Count > myEdgeList.Edges.Count) // if the IDNode is only one level above we can't skip this selection elemen because it is the last one: U.Friends 
                    {
                        return true;
                    }
                    else if (RelatedIDChainDefinition.Edges.Count == myEdgeList.Edges.Count && RelatedIDChainDefinition.IsUndefinedAttribute)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }

    }

    #endregion

    #region SelectionElementFunction

    public sealed class SelectionElementFunction : SelectionElement
    {

        public ChainPartFuncDefinition Function { get; private set; }
        public List<AExpressionDefinition> Parameters { get; private set; }
        public SelectionElementFunction FollowingFunction { get; private set; }

        public SelectionElementFunction(SelectionElement mySelectionElement, ChainPartFuncDefinition myFunction, List<AExpressionDefinition> myParameters)
        {
            this.Alias = mySelectionElement.Alias;
            this.EdgeList = mySelectionElement.EdgeList;
            this.Element = mySelectionElement.Element;
            this.Selection = mySelectionElement.Selection;
            this.IsGroupedOrAggregated = mySelectionElement.IsGroupedOrAggregated;
            this.LevelKey = mySelectionElement.LevelKey;
            this.RelatedIDChainDefinition = mySelectionElement.RelatedIDChainDefinition;

            Function = myFunction;
            Parameters = myParameters;
        }

        public void AddFollowingFunction(SelectionElementFunction myFollowingFunction)
        {
            var curFunc = this;
            while (curFunc.FollowingFunction != null)
            {
                curFunc = curFunc.FollowingFunction;
            }
            curFunc.FollowingFunction = myFollowingFunction;
        }

        public override bool Equals(object obj)
        {

            if (!(obj is SelectionElementFunction))
            {
                return false;
            }

            var otherSelElF = (obj as SelectionElementFunction);

            return Alias == otherSelElF.Alias && EdgeList.Equals(otherSelElF.EdgeList) && Element.Equals(otherSelElF.Element)
                && Selection == otherSelElF.Selection && IsGroupedOrAggregated == otherSelElF.IsGroupedOrAggregated && LevelKey == otherSelElF.LevelKey
                && Function.FuncName == otherSelElF.Function.FuncName && FollowingFunction == otherSelElF.FollowingFunction
                && Parameters.Count == otherSelElF.Parameters.Count && Parameters.All(p => otherSelElF.Parameters.Contains(p));

        }

        public override int GetHashCode()
        {
            return Alias.GetHashCode() ^ EdgeList.GetHashCode() ^ Element.GetHashCode() ^ Selection.GetHashCode() ^ IsGroupedOrAggregated.GetHashCode() ^ LevelKey.GetHashCode() ^ Function.FuncName.GetHashCode();
        }

    }

    #endregion

    #region SelectionElementAggregate

    public sealed class SelectionElementAggregate : SelectionElement
    {

        #region Properties

        public IGQLAggregate Aggregate { get; private set; }
        public AggregateDefinition AggregateDefinition { get; private set; }

        #endregion

        #region Ctor

        public SelectionElementAggregate(IGQLAggregate myBaseAggregate, string myAlias, EdgeList myEdgeList, LevelKey myLevelKey, IDChainDefinition myRelatedIDChainDefinition, AggregateDefinition myAggregateDefinition)
        {
            Alias = myAlias;
            EdgeList = myEdgeList;
            LevelKey = myLevelKey;
            Aggregate = myBaseAggregate;
            AggregateDefinition = myAggregateDefinition;
            RelatedIDChainDefinition = myRelatedIDChainDefinition;
        }

        #endregion

        public override string ToString()
        {
            return RelatedIDChainDefinition.ContentString;
        }

    }

    #endregion

}

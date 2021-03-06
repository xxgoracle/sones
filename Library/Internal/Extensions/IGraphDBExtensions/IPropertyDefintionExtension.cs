﻿using System;
using System.Linq;
using sones.Library.PropertyHyperGraph;
using sones.GraphDB.TypeSystem;
using sones.Constants;

namespace sones.GraphDB.Extensions
{
    public static class IPropertyDefintionExtension
    {

        public static bool HasValue(this IPropertyDefinition myProperty, IGraphElement myElement)
        {
            if (myProperty == null)
                throw new NullReferenceException();

            if (myElement == null)
                throw new ArgumentNullException("myElement");

            if (myElement is IVertex)
                return HasValue(myProperty, myElement as IVertex);
            
            if (myElement is IEdge)
                return HasValue(myProperty, myElement as IEdge);

            throw new Exception("Unknown IGraphElement.");
        }

        public static IComparable GetValue(this IPropertyDefinition myProperty, IGraphElement myElement)
        {
            if (!HasValue(myProperty, myElement))
                return null;

            if (myElement is IVertex)
                return GetValue(myProperty, myElement as IVertex);

            if (myElement is IEdge)
                return GetValue(myProperty, myElement as IEdge);

            throw new Exception("Unknown IGraphElement.");
            
        }

        private static bool HasValue(this IPropertyDefinition myProperty, IEdge myEdge)
        {
            if (myProperty.RelatedType == null)
                throw new ArgumentException("A property with nor related type is not allowed.");

            if (!myProperty.RelatedType.Name.Equals(GlobalConstants.Edge))
                return myEdge.HasProperty(myProperty.ID);

            switch (myProperty.Name)
            {
                case GlobalConstants.EdgeDotComment:
                    return myEdge.Comment != null;

                case GlobalConstants.EdgeDotCreationDate:
                case GlobalConstants.EdgeDotModificationDate:
                case GlobalConstants.EdgeDotEdgeTypeID:
                case GlobalConstants.EdgeDotEdgeTypeName:
                    return true;

                default:
                    throw new Exception(
                        "A new property was added to the edge type Edge, but this switch stement was not changed.");
            }
        }

        private static bool HasValue(this IPropertyDefinition myProperty, IVertex myVertex)
        {
            if (myProperty.RelatedType == null)
                throw new ArgumentException("A property with nor related type is not allowed.");

            if (!myProperty.RelatedType.Name.Equals(GlobalConstants.Vertex))
                return myVertex.HasProperty(myProperty.ID);

            switch (myProperty.Name)
            {
                case GlobalConstants.VertexDotComment:
                    return myVertex.Comment != null;

                case GlobalConstants.VertexDotCreationDate:
                case GlobalConstants.VertexDotEdition:
                case GlobalConstants.VertexDotModificationDate:
                case GlobalConstants.VertexDotRevision:
                case GlobalConstants.VertexDotVertexTypeID:
                case GlobalConstants.VertexDotVertexTypeName:
                case GlobalConstants.VertexDotVertexID:
                    return true;

                default:
                    throw new Exception(
                        "A new property was added to the vertex type Vertex, but this switch stement was not changed.");

            }
        }


        private static IComparable GetValue(this IPropertyDefinition myProperty, IEdge myEdge)
        {
            if (myProperty.RelatedType == null)
                throw new ArgumentException("A property with nor related type is not allowed.");

            if (!myProperty.RelatedType.Name.Equals(GlobalConstants.Edge))
                return myEdge.GetProperty<IComparable>(myProperty.ID);

            switch (myProperty.Name)
            {
                case GlobalConstants.EdgeDotComment:
                    return myEdge.Comment;

                case GlobalConstants.EdgeDotCreationDate:
                    return myEdge.CreationDate;

                case GlobalConstants.EdgeDotModificationDate:
                    return myEdge.ModificationDate;

                case GlobalConstants.EdgeDotEdgeTypeID:
                    return myEdge.EdgeTypeID;

                case GlobalConstants.EdgeDotEdgeTypeName:
                    return myProperty.RelatedType.GetDescendantTypesAndSelf().Where(_ => _.ID == myEdge.EdgeTypeID).Select(__ => __.Name).FirstOrDefault();

                default:
                    throw new System.Exception(
                        "A new property was added to the edeg type Edge, but this switch stement was not changed.");

            }
        }

        private static IComparable GetValue(this IPropertyDefinition myProperty, IVertex myVertex)
        {
            if (myProperty.RelatedType == null)
                throw new ArgumentException("A property with nor related type is not allowed.");

            if (!myProperty.RelatedType.Name.Equals(GlobalConstants.Vertex))
                return myVertex.GetProperty<IComparable>(myProperty.ID);

            switch (myProperty.Name)
            {
                case GlobalConstants.VertexDotComment:
                    return myVertex.Comment;

                case GlobalConstants.VertexDotCreationDate:
                    return myVertex.CreationDate;

                case GlobalConstants.VertexDotEdition:
                    return myVertex.EditionName;

                case GlobalConstants.VertexDotModificationDate:
                    return myVertex.ModificationDate;

                case GlobalConstants.VertexDotRevision:
                    return myVertex.VertexRevisionID;

                case GlobalConstants.VertexDotVertexTypeID:
                    return myVertex.VertexTypeID;

                case GlobalConstants.VertexDotVertexTypeName:
                    return myProperty.RelatedType.GetDescendantTypesAndSelf().Where(_ => _.ID == myVertex.VertexTypeID).Select(__ => __.Name).FirstOrDefault();

                case GlobalConstants.VertexDotVertexID:
                    return myVertex.VertexID;

                default:
                    throw new System.Exception(
                        "A new property was added to the vertex type Vertex, but this switch stement was not changed.");

            }
        }
    }
}

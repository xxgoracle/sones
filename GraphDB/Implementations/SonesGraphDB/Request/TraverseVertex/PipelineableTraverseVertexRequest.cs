﻿using System.Collections.Generic;
using sones.GraphDB.ErrorHandling.Expression;
using sones.GraphDB.Expression;
using sones.GraphDB.Manager;
using sones.Library.PropertyHyperGraph;
using sones.Library.Commons.Security;
using sones.Library.Commons.Transaction;
using sones.GraphDB.TypeSystem;
using System;

namespace sones.GraphDB.Request
{
    /// <summary>
    /// This class is responsible for realizing a traverse on verticies on the database
    /// </summary>
    public sealed class PipelineableTraverseVertexRequest : APipelinableRequest
    {
        #region data

        /// <summary>
        /// The request that contains the todo
        /// </summary>
        private readonly RequestTraverseVertex _request;

        /// <summary>
        /// The verticies which are fetched by the Traverser
        /// it is used for generating the output
        /// </summary>
        private IEnumerable<IVertex> _fetchedIVertices;

        /// <summary>
        /// Traversal state holds statistical informations about the traversion
        /// </summary>
        private TraversalState _traversalState;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new pipelineable traverse vertex request
        /// </summary>
        /// <param name="myTraverseVertexRequest">The traverse vertex options request</param>
        /// <param name="mySecurity">The security token of the request initiator</param>
        /// <param name="myTransactionToken">The myOutgoingEdgeVertex transaction token</param>
        public PipelineableTraverseVertexRequest( RequestTraverseVertex myTraverseVertexRequest, 
                                                  SecurityToken mySecurity,
                                                  TransactionToken myTransactionToken)
            : base(mySecurity, myTransactionToken)
        {
            _request = myTraverseVertexRequest;
        }

        #endregion

        #region APipelinableRequest Members

        /// <summary>
        /// Validates the request / expression
        /// </summary>
        /// <param name="myMetaManager">Provides all important manager</param>
        public override void Validate(IMetaManager myMetaManager)
        {
            if ((_request == null) || (!IsValidExpression(_request.TraverseVertexDefinition.Expression)))
                throw new InvalidExpressionException(_request.TraverseVertexDefinition.Expression);
        }

        /// <summary>
        /// Executes the traversion
        /// </summary>
        /// <param name="myMetaManager">Provides all important manager</param>
        public override void Execute(IMetaManager myMetaManager)
        {
            //get start node by expression and create traversalState
            _traversalState = new TraversalState(myMetaManager.VertexManager.GetSingleVertex(_request.TraverseVertexDefinition.Expression, TransactionToken, SecurityToken));

            //do traversion
            _fetchedIVertices = TraverseVertex_private( _traversalState.StartNode,
                                                        null,
                                                        myMetaManager,
                                                        _request.TraverseVertexDefinition.AvoidCircles,
                                                        _request.TraverseVertexDefinition.FollowThisEdge,
                                                        _request.TraverseVertexDefinition.MatchEvaluator,
                                                        _request.TraverseVertexDefinition.MatchAction,
                                                        _request.TraverseVertexDefinition.StopEvaluator );
        }

        public override IRequest GetRequest()
        {
            return _request;
        }

        #endregion

        #region internal methods

        /// <summary>
        /// Creates the output for a get vertices request
        /// </summary>
        /// <typeparam name="TResult">The type of the myResult</typeparam>
        /// <param name="myOutputconverter">The delegate that is executed uppon output-generation</param>
        /// <returns>A TResult</returns>
        internal TResult GenerateRequestResult<TResult>(Converter.TraverseVertexResultConverter<TResult> myOutputconverter)
        {
            return myOutputconverter(Statistics, _fetchedIVertices);
        }

        #endregion

        #region private

        /// <summary>
        /// The private traverser method
        /// </summary>
        /// <param name="myCurrentVertex">Vertex / start node for the next search step</param>
        /// <param name="myViaVertex">Vertex / predecessor of the start node</param>
        /// <param name="myAvoidCircles">Avoid circles?</param>
        /// <param name="myFollowThisEdge">Function to check if the edge should be followed</param>
        /// <param name="myMatchEvaluator">Function to check match</param>
        /// <param name="myMatchAction">A Action to perform on each match found</param>
        /// <param name="myStopEvaluator">Evaluator to stop traversion</param>
        /// <returns>Enumerable of verticies</returns>
        private IEnumerable<IVertex> TraverseVertex_private( IVertex                                                 myCurrentVertex,
                                                             IVertex                                                 myViaVertex,
                                                             IMetaManager                                            myMetaManager,
                                                             Boolean                                                 myAvoidCircles,
                                                             Func<IVertex, IVertexType, IEdge, IEdgeType, Boolean>   myFollowThisEdge,
                                                             Func<IVertex, IVertexType, Boolean>                     myMatchEvaluator,
                                                             Action<IVertex>                                         myMatchAction,
                                                             Func<TraversalState, Boolean>                           myStopEvaluator )
        {
            #region stop evaluation?
            //are we allowed to stop the current traversal

            if (myStopEvaluator != null)
            {
                if (myStopEvaluator(_traversalState))
                {
                    yield break;
                }
            }

            #endregion

            #region currentVertex match?
            //does the current node match the requirements?

            var match = false;

            #region match evaluation

            if (myMatchEvaluator != null)
            {
                //there is a match evaluator... use it

                if (myMatchEvaluator(myCurrentVertex, myMetaManager.VertexTypeManager.GetVertexType(myCurrentVertex.VertexTypeID, TransactionToken, SecurityToken)))
                {
                    match = true;
                }
            }
            else
            {
                //there is no special function that evaluates if the current vertex matches... so EVERY Vertex matches

                match = true;
            }

            #endregion

            if (match)
            {
                #region match action
                //do the specified match action

                if (myMatchAction != null)
                {
                    myMatchAction.Invoke(myCurrentVertex);
                }

                #endregion

                #region update traversal state and increase count of found elements
                //update number of found elements

                _traversalState.IncreaseNumberOfFoundElements();

                #endregion
            }

            #endregion

            #region update statistics on traversal state

            _traversalState.AddVisitedVertexViaVertex(myCurrentVertex, myViaVertex);

            #endregion

            #region return and traverse
            //get all edges and try to traverse them
            //return results

            if (match)
            {
                //return the current vertex if it matched
                yield return myCurrentVertex;
            }

            #region traverse by using outgoing edges
            //first do recursive search by using the outgoing edges

            foreach (var _OutEdge in myCurrentVertex.GetAllOutgoingEdges())
            {
                var outEdge = _OutEdge.Item2;

                #region check edge
                //check if the edge should be followed... if not, continue!

                if (myFollowThisEdge != null)
                {
                    if (!myFollowThisEdge(myCurrentVertex,
                                            myMetaManager.VertexTypeManager.GetVertexType(myCurrentVertex.VertexTypeID, TransactionToken, SecurityToken),
                                            outEdge,
                                            myMetaManager.EdgeTypeManager.GetEdgeType(outEdge.EdgeTypeID, TransactionToken, SecurityToken)))
                    {
                        continue;
                    }
                }

                #endregion

                var nextVerticies = outEdge.GetTargetVertices();

                #region take every vertex and do recursion

                foreach (var nextVertex in nextVerticies)
                {
                    //check for circle avoidance
                    if (myAvoidCircles)
                    {
                        #region check traversal state
                        //check the traversal state for circles... if there is one, break!

                        if (_traversalState.AlreadyVisitedVertexViaVertex(nextVertex, myCurrentVertex))
                        {
                            continue;
                        }

                        #endregion
                    }

                    //move recursive in depth

                    foreach (var vertex in TraverseVertex_private(nextVertex,
                                                                    myCurrentVertex,
                                                                    myMetaManager,
                                                                    myAvoidCircles,
                                                                    myFollowThisEdge,
                                                                    myMatchEvaluator,
                                                                    myMatchAction,
                                                                    myStopEvaluator))
                    {
                        yield return vertex;
                    }

                }

                #endregion
            }

            #endregion

            #endregion
        }

        #endregion

        #region private helper

        #region IsValidExpression

        /// <summary>
        /// Is the expression valid
        /// </summary>
        /// <param name="myExpression">The to be validated expression</param>
        /// <returns>True or false</returns>
        private bool IsValidExpression(IExpression myExpression)
        {
            switch (myExpression.TypeOfExpression)
            {
                case TypeOfExpression.Binary:
                    return (IsValidExpression(((BinaryExpression)myExpression).Left) && IsValidExpression(((BinaryExpression)myExpression).Right));
                case TypeOfExpression.Property:
                    return (IsValidPropertyExpression((PropertyExpression)myExpression));
                case TypeOfExpression.Constant:
                    return (IsValidConstantExpression((ConstantExpression)myExpression));
                case TypeOfExpression.Unary:
                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if the property expression is valid
        /// </summary>
        /// <param name="propertyExpression">The expression</param>
        /// <returns>True or False</returns>
        private bool IsValidPropertyExpression(PropertyExpression propertyExpression)
        {
            return (propertyExpression.NameOfProperty != null && propertyExpression.NameOfVertexType != null);
        }

        /// <summary>
        /// Checks if the constant expression is valid
        /// </summary>
        /// <param name="constantExpression">The expression</param>
        /// <returns>True or False</returns>
        private bool IsValidConstantExpression(ConstantExpression constantExpression)
        {
            return (constantExpression != null);
        }
        
        #endregion

        #endregion
    }
}
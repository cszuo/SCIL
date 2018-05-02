﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SCIL.Processor.Nodes;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL.Processor.ControlFlow
{
    [RegistrerRewriter]
    public class StackAnalyzerVisitor : BaseVisitor
    {
        public override void Visit(Method method)
        {
            var visitor = new MethodVisitor();
            visitor.Visit(method);
        }

        private class MethodVisitor : BaseVisitor
        {
            private readonly IDictionary<Block, int> _stacks = new Dictionary<Block, int>();
            private int _nextStack = 0;

            public override void Visit(Method method)
            {
                Debug.Assert(_nextStack == 0);

                base.Visit(method);

                // Assert that the stack is handled (next stack should be 0)
                Debug.Assert(_nextStack == 0);
            }

            public override void Visit(Block block)
            {
                // Get stack
                if (block.Sources.Any())
                {
                    var stack = _stacks[block.Sources.First()];
                    _nextStack = stack;
                }
                else
                {
                    _nextStack = 0;
                }

                // Add to stack list
                _stacks[block] = _nextStack;

                // Run visitor
                var visitor = new BlockVisitor(_nextStack);
                visitor.Visit(block);
            }

            private class BlockVisitor : BaseVisitor
            {
                private int _nextStack;

                public BlockVisitor(int nextStack)
                {
                    this._nextStack = nextStack;
                }

                public override void Visit(Node node)
                {
                    // Detect phi nodes
                    if (node is PhiNode phiNode)
                    {
                        var parentPushNames = phiNode.Parents.Select(e => e.PushStack.Last()).Distinct().ToList();

                        // Parent pushnames should all have the same index
                        Debug.Assert(parentPushNames.Count == 1);

                        // Set the push stack
                        node.SetPushStack(parentPushNames.First());

                        base.Visit(node);
                        return;
                    }

                    // Detect exception handling
                    if (node.Block.Method.Definition.Body.ExceptionHandlers.Any(e => e.HandlerStart == node.Instruction))
                    {
                        // Popall
                        _nextStack = 1;
                    }
                    else
                    {
                        node.SetPopStack(GetPopStackNames(node).ToArray());
                        node.SetPushStack(GetPushStackNames(node).ToArray());
                    }
                    
                    base.Visit(node);
                }

                private IEnumerable<int> GetPopStackNames(Node node)
                {
                    for (int i = 0; i < node.GetRequiredNames().popNames; i++)
                    {
                        yield return --_nextStack;
                    }
                    
                    Debug.Assert(_nextStack >= 0);
                }

                private IEnumerable<int> GetPushStackNames(Node node)
                {
                    Debug.Assert(_nextStack >= 0);

                    for (int i = 0; i < node.GetRequiredNames().pushNames; i++)
                    {
                        yield return _nextStack++;
                    }
                }
            }

        }
    }
}

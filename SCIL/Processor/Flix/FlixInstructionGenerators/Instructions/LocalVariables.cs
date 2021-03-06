﻿using System;
using System.Linq;
using Mono.Cecil.Cil;

namespace SCIL.Processor.FlixInstructionGenerators.Instructions
{
    public class LocalVariables : IFlixInstructionGenerator
    {
        public bool GenerateCode(Node node, out string outputFlixCode)
        {
            switch (node.OpCode.Code)
            {
                case Code.Stloc:
                case Code.Stloc_S:
                    if (node.Operand is sbyte)
                    {
                        outputFlixCode = stloc(node);
                        return true;
                    }
                    else if (node.Operand is VariableDefinition)
                    {
                        outputFlixCode = stloc(node);
                        return true;
                    }
                    throw new ArgumentOutOfRangeException(nameof(node.Operand));
                case Code.Ldloc:
                case Code.Ldloc_S:
                    if (node.Operand is sbyte)
                    {
                        outputFlixCode = ldloc(node);
                        return true;
                    }
                    else if (node.Operand is VariableDefinition)
                    {
                        outputFlixCode = ldloc(node);
                        return true;
                    }
                    throw new ArgumentOutOfRangeException(nameof(node.Operand));
                case Code.Ldloca:
                case Code.Ldloca_S:
                    if (node.Operand is sbyte)
                    {
                        outputFlixCode = ldloca(node);
                        return true;
                    }
                    else if (node.Operand is VariableDefinition)
                    {
                        outputFlixCode = ldloca(node);
                        return true;
                    }
                    throw new ArgumentOutOfRangeException(nameof(node.Operand));
            }
            outputFlixCode = null;
            return false;
        }

       // private string stloc(Node node) => $"StlocStm({node.VariableName}, {node.PopStackNames.First()}, \"{GetVariableDefinition(node).VariableType.FullName}\").";
        private string stloc(Node node) => $"StlocStm({node.VariableName}, {node.PopStackNames.First()}).";
        private string ldloc(Node node) => $"LdlocStm({node.PushStackNames.First()}, {node.VariableName}).";
        private string ldloca(Node node) => $"LdlocaStm({node.PushStackNames.First()}, {node.VariableName}).";

        private VariableDefinition GetVariableDefinition(Node node)
        {
            if (node.Operand is VariableDefinition variableDefinition)
                return variableDefinition;

            var index = GetVariableIndex(node.Operand);
            return node.Block.Method.Definition.Body.Variables.First(e => e.Index == index);
        }
        private int GetVariableIndex(object operand) {
            if (operand is sbyte index)
            {
                return index;
            }
            else if (operand is VariableDefinition variableDefinition)
            {
                return variableDefinition.Index;
            }

            throw new NotImplementedException();
        }
    }
}
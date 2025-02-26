﻿using System.Diagnostics;

namespace RetroDev.UISharp.UIDefinition.Ast;

[DebuggerDisplay("{Name}")]
public class Component(string name, List<Attribute> attributes, List<Component> components) : Node
{
    public string Name { get; set; } = name;
    public List<Attribute> Attributes { get; set; } = attributes;
    public List<Component> Components { get; set; } = components;
}

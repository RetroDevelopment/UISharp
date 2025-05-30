﻿using System.Reflection;
using RetroDev.UISharp.Components.Collections;
using RetroDev.UISharp.Components.Containers;
using RetroDev.UISharp.Components.Core;
using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Components.Layouts;
using RetroDev.UISharp.Components.Simple;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.IDE.Components;
using RetroDev.UISharp.Presentation.Properties;
using RetroDev.UISharp.UIDefinition.Ast;
using RetroDev.UISharp.Windows;
using Attribute = RetroDev.UISharp.UIDefinition.Ast.Attribute;

namespace RetroDev.UISharp.IDE.Windows;

// TODO: check for unique ids

[EditorSettings(allow: false)]
internal class Container : UIObject
{
    public Container(Application application, List<UIControl> children) : base(application)
    {
        children.ForEach(Children.Add);
    }

    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) => new(100, 100);
}

internal class MainWindow : Window
{
    private Dictionary<UITreeNode<UIControl>, Component> _treeNodeAstMap = [];
    private Component? _rootNode;

    private readonly GridLayout _mainLayout;
    private readonly EditBox _fileEditBox;
    private readonly ListBox _components;
    private readonly TreeBox _astTreeBox;
    private readonly Button _loadButton;
    private readonly Button _saveButton;
    private readonly Button _refreshButton;
    private readonly Button _addButton;
    private readonly Button _removeButton;
    private readonly Switch _darkMode;
    private readonly ListBox _propertyList;

    public MainWindow(Application parent,
                      GridLayout mainLayout,
                      ListBox components,
                      EditBox file,
                      TreeBox astTreeBox,
                      Button load,
                      Button save,
                      Button refresh,
                      Button add,
                      Button remove,
                      Switch darkMode,
                      ListBox propertyList) : base(parent)
    {
        Initialized += MainWindow_Initialized;
        _mainLayout = mainLayout;
        _components = components;
        _fileEditBox = file;
        _astTreeBox = astTreeBox;
        _loadButton = load;
        _saveButton = save;
        _refreshButton = refresh;
        _addButton = add;
        _removeButton = remove;
        _darkMode = darkMode;
        _propertyList = propertyList;
    }

    private void MainWindow_Initialized(Window sender, EventArgs e)
    {
        InitializeComponentListBox();
        InitializeButtons();
        _astTreeBox.SelectedNode.ValueChange.Subscribe(OnTreeNodeChange);
        _components.SelectedItem.ValueChange.Subscribe(_ => UpdateAddRemoveButtonState());
        InitializeTitle();
    }

    private void InitializeComponentListBox()
    {
        var typeMapper = Application.UIDefinitionManager.TypeMapper;
        var componentTypes = typeMapper.GetComponentTypes();
        foreach (var componentType in componentTypes.Where(FilterTypesByEditorSettings))
        {
            var label = new Label(Application);
            label.Text.Value = componentType.Name;
            label.AutoWidth.Value = AutoSize.Wrap;
            label.AutoHeight.Value = AutoSize.Wrap;
            label.HorizontalAlignment.Value = Alignment.Left;
            label.VerticalAlignment.Value = Alignment.Top;
            _components.Items.Add(label);
        }
    }

    private void InitializeButtons()
    {
        _loadButton.Action += LoadButton_Action;
        _saveButton.Action += SaveButton_Action;
        _refreshButton.Action += RefreshButton_Action;
        _addButton.Action += AddButton_Action;
        _removeButton.Action += RemoveButton_Action;
        _darkMode.Checked.ValueChange.Subscribe(OnCheckedChange);
    }

    private void InitializeTitle()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        var cleanVersion = informationalVersion!.Split('+')[0];
        Title.Value += $" v{cleanVersion}";
    }

    private void LoadButton_Action(Button sender, EventArgs e)
    {
        LoadXml(_fileEditBox.Text.Value);
        _saveButton.Enabled.Value = true;
        _refreshButton.Enabled.Value = true;
    }

    private void SaveButton_Action(Button sender, EventArgs e)
    {
        var xml = Application.UIDefinitionManager.CodeGenerator.Generate(_rootNode!);
        File.WriteAllText(GetUIDefinitionFullPath(_fileEditBox.Text.Value), xml);
    }

    private void RefreshButton_Action(Button sender, EventArgs e)
    {
        // Refresh is ONLY needed for when adding or removing text to an attribute (x, y, width, etc.)
        // TODO: remove refresh and use auto refresh. But that requires that attributes are not just edit boxes, but that there is input validation.
        CreateComponentInstance();
    }

    private void AddButton_Action(Button sender, EventArgs e)
    {
        if (_astTreeBox.SelectedNode == null || _components.SelectedItem == null) throw new InvalidOperationException();
        var selectedItem = _components?.SelectedItem?.Value ?? throw new Exception("Cannot add item if no selected item");
        var selectedNode = _astTreeBox.SelectedNode;
        var componentToAdd = _components.SelectedItem;

        var componentName = ((Label)selectedItem).Text.Value;
        var childNode = CreateNode(componentName);
        var astChildNode = new Component(componentName, [], []);
        var parent = selectedNode.Value;
        if (parent is not null)
        {
            var astParent = _treeNodeAstMap[parent];
            astParent.Components.Add(astChildNode);
            AddNode(parent, astChildNode);
        }
        else
        {
            _rootNode!.Components.Add(astChildNode);
            AddNode(null, astChildNode);
        }

        CreateComponentInstance();
    }

    private void RemoveButton_Action(Button sender, EventArgs e)
    {
        if (_astTreeBox.SelectedNode == null || _components.SelectedItem == null) throw new InvalidOperationException();
        var selectedNode = _astTreeBox.SelectedNode.Value;
        var parent = selectedNode!.Parent;
        var astSelectedNode = _treeNodeAstMap[selectedNode];

        if (parent is not null)
        {
            var astParent = _treeNodeAstMap[parent];
            astParent.Components.Remove(astSelectedNode);
            parent.Children.Remove(selectedNode);
        }
        else
        {
            _astTreeBox.Items.Children.Remove(selectedNode);
            _rootNode!.Components.Remove(astSelectedNode);
            _treeNodeAstMap.Remove(selectedNode);
        }

        _treeNodeAstMap.Remove(selectedNode);
        CreateComponentInstance();
    }

    private void OnCheckedChange(bool @checked)
    {
        if (@checked)
        {
            Application.ThemeManager.LoadTheme("uisharp-dark");
        }
        else
        {
            Application.ThemeManager.LoadTheme("uisharp-light");
        }
    }

    private void OnTreeNodeChange(UITreeNode<UIControl>? node)
    {
        var listBox = _propertyList;
        listBox.Items.Clear();

        if (node == null)
        {
            UpdateAddRemoveButtonState();
            return;
        }

        var selectedAstNode = _treeNodeAstMap[node];
        var typeMapper = Application.UIDefinitionManager.TypeMapper;
        var type = typeMapper.GetUIComponent(selectedAstNode.Name);
        var properties = typeMapper.GetProperties(type!);
        foreach (var property in properties)
        {
            var gridLayout = new GridLayout(Application);
            gridLayout.Rows.Value = 1;
            gridLayout.Columns.Value = 2;

            var header = new Label(Application, property.Name);
            header.AutoWidth.Value = AutoSize.Wrap;
            header.HorizontalAlignment.Value = Alignment.Left;

            var editBox = new EditBox(Application);
            editBox.ID.Value = property.Name;
            editBox.AutoWidth.Value = AutoSize.Stretch;

            var attribute = selectedAstNode.Attributes.Where(c => c.Name.Equals(property.Name, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            if (attribute is not null)
            {
                editBox.Text.Value = attribute.Value;
            }
            else
            {
                attribute = new Attribute(property.Name, string.Empty);
            }

            editBox.Text.ValueChange.Subscribe(text => OnAttributeChange(selectedAstNode, attribute, text));

            gridLayout.Items.Add(header);
            gridLayout.Items.Add(editBox);
            gridLayout.Height.Value = 30;
            listBox.Items.Add(gridLayout);
        }

        UpdateAddRemoveButtonState();
    }

    private void OnAttributeChange(Component node, UISharp.UIDefinition.Ast.Attribute attribute, string value)
    {
        attribute.Value = value;

        if (value == string.Empty)
        {
            node.Attributes.Remove(attribute);
        }
        else if (!node.Attributes.Any(a => a == attribute))
        {
            node.Attributes.Add(attribute);
        }
    }

    private void LoadXml(string fileName)
    {
        var xml = File.ReadAllText(GetUIDefinitionFullPath(fileName));
        _rootNode = Application.UIDefinitionManager.Parser.Parse(xml);
        ClearTreeBox();
        AddNode(null, _rootNode);
        CreateComponentInstance();
    }

    private string GetUIDefinitionFullPath(string fileName)
    {
        // TODO: use full path or file loaders. This is a hard coded file name that works differently on Linux.
        var windows = "..\\..\\..\\Windows\\" + fileName;
        if (File.Exists(windows)) return windows;
        else return "../../../../../Windows/" + fileName;
    }

    private void CreateComponentInstance()
    {
        var components = _rootNode!.Components.Select(Application.UIDefinitionManager.InstanceCreator.CreateUIComponent).Cast<UIControl>().ToList();
        var container = new UIPreview(Application, components);
        _mainLayout.GetComponent<ScrollView>("preview").Item.Value = container;
    }

    private UITreeNode<UIControl> CreateNode(string text)
    {
        var label = new Label(Application, text);
        var node = new UITreeNode<UIControl>(Application, label);
        label.AutoWidth.Value = AutoSize.Wrap;
        label.AutoHeight.Value = AutoSize.Wrap;
        label.HorizontalAlignment.Value = Alignment.Left;
        return node;
    }

    private void AddNode(UITreeNode<UIControl>? parent, Component astNode)
    {
        var node = CreateNode(astNode.Name);
        _treeNodeAstMap.Add(node, astNode);

        if (parent is not null)
        {
            parent.Children.Add(node);
        }
        else
        {
            _astTreeBox.Items.Children.Add(node);
        }

        astNode.Components.ForEach(child => { AddNode(node, child); });
    }

    private void UpdateAddRemoveButtonState()
    {
        _removeButton.Enabled.Value = _astTreeBox.SelectedNode.Value is not null;

        if (_astTreeBox.SelectedNode.Value is not null && _components.SelectedItem.Value is not null)
        {
            var name = ((Label)(_astTreeBox.SelectedNode.Value.Content.Value)).Text.Value;
            var type = Application.UIDefinitionManager.TypeMapper.GetUIComponent(name);
            if (type == null) throw new Exception($"Cannot find type for component {name} to add.");
            _addButton.Enabled.Value = (type.GetInterfaces().Contains(typeof(IContainer)));
        }
    }

    private static bool FilterTypesByEditorSettings(Type type)
    {
        // Get the EditorSettings attribute
        var attribute = type.GetCustomAttribute<EditorSettingsAttribute>();
        // Include the type if there's no attribute or Allow == true
        return attribute == null || attribute.Allow;
    }

    private void ClearTreeBox()
    {
        _astTreeBox.Items.Children.Clear();
        _treeNodeAstMap.Clear();
    }
}

using System.Reflection;
using RetroDev.OpenIDE.Components;
using RetroDev.OpenUI;
using RetroDev.OpenUI.Components;
using RetroDev.OpenUI.Components.Base;
using RetroDev.OpenUI.Components.Containers;
using RetroDev.OpenUI.Components.Core.AutoArea;
using RetroDev.OpenUI.Components.Simple;
using RetroDev.OpenUI.Core.Graphics;
using RetroDev.OpenUI.UI.Coordinates;
using RetroDev.OpenUI.UI.Properties;
using RetroDev.OpenUI.UIDefinition.Ast;
using Attribute = RetroDev.OpenUI.UIDefinition.Ast.Attribute;

namespace RetroDev.OpenIDE.Windows;

// TODO: GetComponent<T> should be recursive to avoid the hell of nested components lookup
// TODO: check for unique ids

[EditorSettings(allow: false)]
internal class Container : UIComponent
{
    public Container(Application application, List<UIWidget> children) : base(application)
    {
        children.ForEach(c => AddChildNode(c));
    }

    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) => new(100, 100);
}

internal class MainWindow : Window
{
    private Dictionary<TreeNode, Component> _treeNodeAstMap = [];
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
    private readonly CheckBox _darkMode;
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
                      CheckBox darkMode,
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
        _astTreeBox.SelectedNode.ValueChange += SelectedNode_ValueChange;
        _components.SelectedItem.ValueChange += SelectedItem_ValueChange;
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
            _components.AddComponent(label);
        }
    }

    private void InitializeButtons()
    {
        _loadButton.Action += LoadButton_Action;
        _saveButton.Action += SaveButton_Action;
        _refreshButton.Action += RefreshButton_Action;
        _addButton.Action += AddButton_Action;
        _removeButton.Action += RemoveButton_Action;
        _darkMode.Checked.ValueChange += Checked_ValueChange;
    }

    private void LoadButton_Action(Button sender, EventArgs e)
    {
        LoadXml(_fileEditBox.Text.Value);
        _saveButton.Enabled.Value = true;
        _refreshButton.Enabled.Value = true;
        FullScreen.Value = !FullScreen.Value;
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
        if (parent != null)
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

        if (parent != null)
        {
            var astParent = _treeNodeAstMap[parent];
            astParent.Components.Remove(astSelectedNode);
            parent.RemoveChild(selectedNode);
        }
        else
        {
            _astTreeBox.RemoveTreeNode(selectedNode);
            _rootNode!.Components.Remove(astSelectedNode);
            _treeNodeAstMap.Remove(selectedNode);
        }

        _treeNodeAstMap.Remove(selectedNode);
        CreateComponentInstance();
    }

    private void Checked_ValueChange(BindableProperty<bool> sender, ValueChangeEventArgs<bool> e)
    {
        if (e.CurrentValue)
        {
            Application.LoadThemeResource("openui-dark");
        }
        else
        {
            Application.LoadThemeResource("openui-light");
        }
    }

    private void SelectedNode_ValueChange(BindableProperty<TreeNode?> sender, ValueChangeEventArgs<TreeNode?> e)
    {
        var listBox = _propertyList;
        listBox.Clear();

        if (e.CurrentValue == null)
        {
            UpdateAddRemoveButtonState();
            return;
        }

        var selectedAstNode = _treeNodeAstMap[e.CurrentValue];
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
            if (attribute != null)
            {
                editBox.Text.Value = attribute.Value;
            }
            else
            {
                attribute = new Attribute(property.Name, string.Empty);
            }

            editBox.Text.ValueChange += (_, e) => OnAttributeChange(selectedAstNode, attribute, e.CurrentValue);

            gridLayout.AddComponent(header);
            gridLayout.AddComponent(editBox);
            gridLayout.Height.Value = 30;
            listBox.AddComponent(gridLayout);
        }

        UpdateAddRemoveButtonState();
    }

    private void SelectedItem_ValueChange(BindableProperty<UIWidget?> sender, ValueChangeEventArgs<UIWidget?> e)
    {
        UpdateAddRemoveButtonState();
    }

    private void OnAttributeChange(Component node, OpenUI.UIDefinition.Ast.Attribute attribute, string value)
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
        var components = _rootNode!.Components.Select(Application.UIDefinitionManager.InstanceCreator.CreateUIComponent).Cast<UIWidget>().ToList();
        var container = new UIPreview(Application, components);
        _mainLayout.GetComponent<ScrollView>("preview").SetComponent(container);
    }

    private TreeNode CreateNode(string text)
    {
        var label = new Label(Application, text);
        var node = new TreeNode(label);
        return node;
    }

    private void AddNode(TreeNode? parent, Component astNode)
    {
        var node = CreateNode(astNode.Name);
        _treeNodeAstMap.Add(node, astNode);

        if (parent != null)
        {
            parent.AddChild(node);
        }
        else
        {
            _astTreeBox.AddTreeNode(node);
        }

        astNode.Components.ForEach(child => { AddNode(node, child); });
    }

    private void UpdateAddRemoveButtonState()
    {
        _removeButton.Enabled.Value = _astTreeBox.SelectedNode.Value != null;

        if (_astTreeBox.SelectedNode.Value != null && _components.SelectedItem.Value != null)
        {
            var name = ((Label)(_astTreeBox.SelectedNode.Value.Content.Value)).Text.Value;
            var type = Application.UIDefinitionManager.TypeMapper.GetUIComponent(name);
            if (type == null) throw new Exception($"Cannot find type for component {name} to add.");
            _addButton.Enabled.Value = (type.GetInterfaces().Contains(typeof(IGenericContainer)));
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
        _astTreeBox.Clear();
        _treeNodeAstMap.Clear();
    }
}

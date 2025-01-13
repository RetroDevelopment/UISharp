using System.Reflection;
using RetroDev.OpenIDE.Components;
using RetroDev.OpenUI;
using RetroDev.OpenUI.Components;
using RetroDev.OpenUI.Components.AutoSize;
using RetroDev.OpenUI.Components.Containers;
using RetroDev.OpenUI.Components.Simple;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Properties;
using RetroDev.OpenUI.UIDefinition.Ast;
using Attribute = RetroDev.OpenUI.UIDefinition.Ast.Attribute;

namespace RetroDev.OpenIDE.Windows;

// TODO: GetComponent<T> should be recursive to avoid the hell of nested components lookup
// TODO: check for unique ids

[EditorSettings(allow: false)]
internal class Container : UIComponent
{
    public Container(Application application, List<UIComponent> children) : base(application)
    {
        children.ForEach(c => AddChild(c));
        AutoWidth.Value = AutoSizeStrategy.MatchParent;
        AutoHeight.Value = AutoSizeStrategy.MatchParent;
    }

    protected override Size ComputeSizeHint() => new(100, 100);
}

internal class MainWindow : Window
{
    private Dictionary<TreeNode, Component> _treeNodeAstMap = [];
    private Component? _rootNode;

    private GridLayout _mainLayout;
    private GridLayout _controlLayout;
    private EditBox _fileEditBox;
    private ListBox _components;
    private TreeBox _astTreeBox;
    private Button _loadButton;
    private Button _saveButton;
    private Button _refreshButton;
    private Button _addButton;
    private Button _removeButton;

    public MainWindow(Application parent,
                      GridLayout mainLayout,
                      GridLayout controlLayout,
                      ListBox components,
                      EditBox file,
                      TreeBox astTreeBox,
                      Button load,
                      Button save,
                      Button refresh,
                      Button add,
                      Button remove) : base(parent)
    {
        Initialized += MainWindow_Initialized;
        _mainLayout = mainLayout;
        _controlLayout = controlLayout;
        _components = components;
        _fileEditBox = file;
        _astTreeBox = astTreeBox;
        _loadButton = load;
        _saveButton = save;
        _refreshButton = refresh;
        _addButton = add;
        _removeButton = remove;
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
            label.AutoWidth.Value = AutoSizeStrategy.WrapComponentLeftTop;
            label.AutoHeight.Value = AutoSizeStrategy.WrapComponentLeftTop;
            _components!.AddComponent(label);
        }
    }

    private void InitializeButtons()
    {
        _loadButton.Action += LoadButton_Action;
        _saveButton.Action += SaveButton_Action;
        _refreshButton.Action += RefreshButton_Action;
        _addButton.Action += AddButton_Action;
        _removeButton.Action += RemoveButton_Action;
    }

    private void LoadButton_Action(Button sender, EventArgs e)
    {
        LoadXml(_fileEditBox!.Text.Value);
        _saveButton!.Enabled.Value = true;
        _refreshButton!.Enabled.Value = true;
    }

    private void SaveButton_Action(Button sender, EventArgs e)
    {
        var xml = Application.UIDefinitionManager.CodeGenerator.Generate(_rootNode!);
        File.WriteAllText(GetUIDefinitionFullPath(_fileEditBox!.Text), xml);
    }

    private void RefreshButton_Action(Button sender, EventArgs e)
    {
        // Refresh is ONLY needed for when adding or removing text to an attribute (x, y, width, etc.)
        // TODO: remove refresh and use auto refresh. But that requires that attributes are not just edit boxes, but that there is input validation.
        CreateComponentInstance();
    }


    private void AddButton_Action(Button sender, EventArgs e)
    {
        if (_astTreeBox!.SelectedNode == null || _components!.SelectedItem == null) throw new InvalidOperationException();

        var selectedNode = _astTreeBox.SelectedNode;
        var componentToAdd = _components.SelectedItem;

        var componentName = ((Label)_components!.SelectedItem).Text;
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
        if (_astTreeBox!.SelectedNode == null || _components!.SelectedItem == null) throw new InvalidOperationException();
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

    private void SelectedNode_ValueChange(TreeBox sender, ValueChangeEventArgs<TreeNode?> e)
    {
        var scrollView = _mainLayout!.GetComponent<ScrollView>("propertiesScrollView");
        var listBox = scrollView.GetComponent<ListBox>("propertyList");
        listBox.Clear();

        if (e.CurrentValue == null) return;

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
            header.AutoWidth.Value = AutoSizeStrategy.WrapComponentLeftTop;

            var editBox = new EditBox(Application);
            editBox.ID.Value = property.Name;
            editBox.AutoWidth.Value = AutoSizeStrategy.MatchParent;
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

            UpdateAddRemoveButtonState();
        }
    }

    private void SelectedItem_ValueChange(ListBox sender, ValueChangeEventArgs<UIComponent?> e)
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
        var components = _rootNode.Components.Select(Application.UIDefinitionManager.InstanceCreator.CreateUIComponent).ToList();
        var container = new UIPreview(Application, components);
        _mainLayout!.GetComponent<ScrollView>("preview").SetComponent(container);
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
            _astTreeBox!.AddTreeNode(node);
        }

        astNode.Components.ForEach(child => { AddNode(node, child); });
    }

    private void UpdateAddRemoveButtonState()
    {
        _removeButton!.Enabled.Value = _astTreeBox!.SelectedNode.Value != null;

        if (_astTreeBox.SelectedNode.Value != null && _components!.SelectedItem.Value != null)
        {
            var name = ((Label)(_astTreeBox.SelectedNode.Value.Content)).Text;
            var type = Application.UIDefinitionManager.TypeMapper.GetUIComponent(name);
            _addButton!.Enabled.Value = (type!.GetInterfaces().Contains(typeof(IGenericContainer)));
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
        _astTreeBox!.Clear();
        _treeNodeAstMap.Clear();
    }
}

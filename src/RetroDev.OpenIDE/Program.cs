using RetroDev.OpenIDE.Windows;
using RetroDev.OpenUI;

namespace RetroDev.OpenIDE;

// Second MVP target: are uncertain things feasable?
// O. Understand why grid layout has 1 pixel margin between components!
// O. Test on Linux?
// O. Implement rounded rectangles, borders, etc. in OpenGL
// O. Implement C++ bindings for getting the input key characters depending on layout
// O. UIComponent split into ApplicationComponent (with only Application class) and derived UIComponent and MVVPComponent.
// O. Implement better binders and UIPropertyList<>, maybe call it BindableProperty
// O. Implement windows resizing and events
// O. More windows events and features (window size, title etc.)
// O. Have AutoSize -> (AutoWidth{hint,stretch}, AutoHeight{hint,stretch}, HorizontalAlignment{left, center, right}, VerticalAlignment{top, center, bottom} -> maybe extend xml? <button autoSize.autoWidth="hint" /> or <button autosize="(hint,hint)(left,center)" />
// O. Do we need ClipArea? Isn't it the same as AbsoluteSize?

// Third MVP target: all the rest to make a finished MVP

//DONE
// X. Manage fous for components. 2 properties: IsFocusable and Focus.
// X. Implement UIComponent.RequestFocus(UIComponent); which will manage focus change from current component to new one.
// X. Implement EditBox component with a focus when clicking and insert + delete. Keep it simple
// X. Implement CheckBox (keep it simple, Checked true or false and simple image) and ProgressBar
// X. Implement ScrollView! that will be used for many components
// X. XML to define UI
// X. Implement asset reader
// X. Better exceptions (for xml parsing for example and assetNOtFound exception)
// X. Implement ListBox
// X. Implement TreeView
// Obis (basic MVP). => Implement minimal OpenIDE app that reads a xml and displays it (+ file system watcher to sync UI in real time)

//TODO
// O. Implement rounded rectangles, borders, etc. in OpenGL
// O. Implement C++ bindings for getting the input key characters depending on layout
// O. Implement windows resizing and events
// O. More windows events and features (window size, title etc.)
// O. Implement logic for checking loops (e.g. SizeHint does not call RelativeDrawingArea, AddChild() do not create loops, etc.)

// O. Implement ContextMenu
// O. Implement layouts! GridLayout, VerticalLayout, HorizontalLayout, GridBagLayout, etc.
// 8. Implement DropDown
// 9. Implement margins in all compoentns
// O. Implement colors and theming
// O. Implement text resources and languages
// O. Implement font size and font selection from file
// ===
// O. Implement button logic (focus colors, 3 states clicking, etc.)
// O. Implement logging
// O. Implement complex focus logic (focus change on tab finding the next focusable object, and maybe focus groups).
// O. Implement dispatcher and syncrhonization context WPF
// => cleanup (e.g. remove svg converter, revisit interfaces: no need for skia interfaces?)

// 1a. Implement logging
// 2a. Implement look and feel
// 3a. C++ platform specifics (modal windows, convert keyboard to character based on window layout, special handling of window resizing for Windows 10/11)
// 4a. SVG to Opengl?
// 5a. Optimize! (texture atlas, instance rendering, retained mode with glScissor to update only part of the screen)
// 6a. Versioning and build pipelines?
// 7a. Validators? Maybe intercept valueChange of components to add validation logic. Something builtin in framework?
// 8a. Material design? Like add PBR and lighting.
// 9a. Logging

// SELLING POINTS
// 1. OpenGL and svg vector graphics (efficient, high quality, support for 3d rendering)
// 2. Allow flexible UI (dynamically load xml UI)
// 3. Simplicity for medium size projects (easier than WPF)
// 4. Cross platofrm (Window, mac, linux)
// 5. Testable (create UI integration tests)

internal class Program
{
    static void Main(string[] _)
    {
        var application = new Application();
        application.ShowWindow<MainWindow>();
        application.Run();
    }
}

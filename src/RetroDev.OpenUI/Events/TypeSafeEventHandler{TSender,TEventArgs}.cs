namespace RetroDev.OpenUI.Events;

public delegate void TypeSafeEventHandler<TSender, TEventArgs>(TSender sender, TEventArgs e);

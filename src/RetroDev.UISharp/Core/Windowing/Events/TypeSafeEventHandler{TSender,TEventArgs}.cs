﻿namespace RetroDev.UISharp.Core.Windowing.Events;

public delegate void TypeSafeEventHandler<TSender, TEventArgs>(TSender sender, TEventArgs e);

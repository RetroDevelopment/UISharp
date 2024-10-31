// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenUI.Core;

/// <summary>
/// An image in the RGBA format. Each 4 byte group represents a pixel red, green, blue, alpha values.
/// </summary>
/// <param name="Data">The image rgba data.</param>
/// <param name="Width">The image width in pixels.</param>
/// <param name="Height">The image height in pixels.</param>
public record RgbaImage(byte[] Data, CoordinateType Width, CoordinateType Height);

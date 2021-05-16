// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System;

namespace SOSIEL.Randoms
{
    public sealed class LinearUniformRandom
    {
        private static Random _random = new Random();

        public static Random GetInstance { get => _random; }

        private LinearUniformRandom() { }
    }
}

// Copyright (C) 2021 SOSIEL Inc. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

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

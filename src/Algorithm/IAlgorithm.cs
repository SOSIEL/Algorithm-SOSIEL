// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

namespace SOSIEL.Algorithm
{
    public interface IAlgorithm<TData>
    {
        string Name { get; }

        void Initialize(TData data);

        TData Run(TData data);
    }
}

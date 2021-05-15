// Copyright (C) 2018-2021 The SOSIEL Foundation. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

namespace SOSIEL.Algorithm
{
    public interface IAlgorithm<TData>
    {
        string Name { get; }

        void Initialize(TData data);

        TData Run(TData data);
    }
}

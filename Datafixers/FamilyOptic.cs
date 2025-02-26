// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers;

public interface FamilyOptic<A, B> {
    TypedOptic<?, ?, A, B> apply(final int index);
}

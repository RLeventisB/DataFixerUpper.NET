// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers.types.families;

using DataFixerUpper.Datafixers.FamilyOptic;
using DataFixerUpper.Datafixers.TypedOptic;
using DataFixerUpper.Datafixers.types.Type;

using java.util.function.IntFunction;

public interface TypeFamily {
    Type<?> apply(final int index);

    static <A, B> FamilyOptic<A, B> familyOptic(final IntFunction<TypedOptic<?, ?, A, B>> optics) {
        return optics::apply;
    }
}

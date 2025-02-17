// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers.types.constant;

using DataFixerUpper.Datafixers.DSL;
using DataFixerUpper.Datafixers.types.Type;
using DataFixerUpper.Datafixers.types.templates.TypeTemplate;
using DataFixerUpper.Datafixers.util.Unit;
using DataFixerUpper.Serialization.Codec;
using DataFixerUpper.Serialization.DynamicOps;

using java.util.Optional;

public final class EmptyPart extends Type<Unit> {
    @Override
    public String toString() {
        return "EmptyPart";
    }

    @Override
    public Optional<Unit> point(final DynamicOps<?> ops) {
        return Optional.of(Unit.INSTANCE);
    }

    @Override
    public boolean equals(final Object o, final boolean ignoreRecursionPoints, final boolean checkIndex) {
        return this == o;
    }

    @Override
    public TypeTemplate buildTemplate() {
        return DSL.constType(this);
    }

    @Override
    protected Codec<Unit> buildCodec() {
        return Codec.EMPTY.codec();
    }
}

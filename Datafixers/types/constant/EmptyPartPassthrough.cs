// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers.types.constant;

using DataFixerUpper.Datafixers.DSL;
using DataFixerUpper.Datafixers.types.Type;
using DataFixerUpper.Datafixers.types.templates.TypeTemplate;
using DataFixerUpper.Serialization.Codec;
using DataFixerUpper.Serialization.Dynamic;
using DataFixerUpper.Serialization.DynamicOps;

using java.util.Optional;

public final class EmptyPartPassthrough extends Type<Dynamic<?>> {
    @Override
    public String toString() {
        return "EmptyPartPassthrough";
    }

    @Override
    public Optional<Dynamic<?>> point(final DynamicOps<?> ops) {
        return Optional.of(new Dynamic<>(ops));
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
    public Codec<Dynamic<?>> buildCodec() {
        return Codec.PASSTHROUGH;
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers.functions;

using DataFixerUpper.Datafixers.types.Type;
using DataFixerUpper.Serialization.DynamicOps;

using java.util.function.Function;

final class Id<A> extends PointFree<Function<A, A>> {
    private final Type<Function<A, A>> type;

    Id(final Type<Function<A, A>> type) {
        this.type = type;
    }

    @Override
    public Type<Function<A, A>> type() {
        return type;
    }

    @Override
    public boolean equals(final Object obj) {
        return obj instanceof Id<?> id && type.equals(id.type);
    }

    @Override
    public int hashCode() {
        return type.hashCode();
    }

    @Override
    public String toString(final int level) {
        return "id";
    }

    @Override
    public Function<DynamicOps<?>, Function<A, A>> eval() {
        return ops -> Function.identity();
    }
}

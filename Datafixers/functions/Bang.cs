// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers.functions;

using DataFixerUpper.Datafixers.DSL;
using DataFixerUpper.Datafixers.types.Type;
using DataFixerUpper.Datafixers.util.Unit;
using DataFixerUpper.Serialization.DynamicOps;

using java.util.function.Function;

final class Bang<A> extends PointFree<Function<A, Unit>> {
    private final Type<A> type;

    Bang(final Type<A> type) {
        this.type = type;
    }

    @Override
    public Type<Function<A, Unit>> type() {
        return DSL.func(type, DSL.emptyPartType());
    }

    @Override
    public String toString(final int level) {
        return "!";
    }

    @Override
    public boolean equals(final Object o) {
        return o instanceof Bang<?> bang && type.equals(bang.type);
    }

    @Override
    public int hashCode() {
        return type.hashCode();
    }

    @Override
    public Function<DynamicOps<?>, Function<A, Unit>> eval() {
        return ops -> a -> Unit.INSTANCE;
    }
}

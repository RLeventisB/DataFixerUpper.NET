// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers.functions;

using DataFixerUpper.Datafixers.DSL;
using DataFixerUpper.Datafixers.types.Type;
using DataFixerUpper.Datafixers.types.templates.RecursivePoint;
using DataFixerUpper.Serialization.DynamicOps;

using java.util.Objects;
using java.util.function.Function;

final class In<A> extends PointFree<Function<A, A>> {
    protected final RecursivePoint.RecursivePointType<A> type;

    public In(final RecursivePoint.RecursivePointType<A> type) {
        this.type = type;
    }

    @Override
    public Type<Function<A, A>> type() {
        return DSL.func(type.unfold(), type);
    }

    @Override
    public String toString(final int level) {
        return "In[" + type + "]";
    }

    @Override
    public boolean equals(final Object obj) {
        if (this == obj) {
            return true;
        }
        return obj instanceof In<?> && Objects.equals(type, ((In<?>) obj).type);
    }

    @Override
    public int hashCode() {
        return type.hashCode();
    }

    @Override
    public Function<DynamicOps<?>, Function<A, A>> eval() {
        return ops -> Function.identity();
    }
}

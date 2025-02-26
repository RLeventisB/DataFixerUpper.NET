// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers.functions;

using DataFixerUpper.Datafixers.DSL;
using DataFixerUpper.Datafixers.FunctionType;
using DataFixerUpper.Datafixers.TypedOptic;
using DataFixerUpper.Datafixers.kinds.App2;
using DataFixerUpper.Datafixers.types.Type;
using DataFixerUpper.Serialization.DynamicOps;

using java.util.Objects;
using java.util.function.Function;

final class ProfunctorTransformer<S, T, A, B> extends PointFree<Function<Function<A, B>, Function<S, T>>> {
    protected final TypedOptic<S, T, A, B> optic;

    public ProfunctorTransformer(final TypedOptic<S, T, A, B> optic) {
        this.optic = optic;
    }

    public <S2, T2> ProfunctorTransformer<S2, T2, A, B> castOuterUnchecked(final Type<S2> sType, final Type<T2> tType) {
        return new ProfunctorTransformer<>(optic.castOuterUnchecked(sType, tType));
    }

    @Override
    public Type<Function<Function<A, B>, Function<S, T>>> type() {
        return DSL.func(DSL.func(optic.aType(), optic.bType()), DSL.func(optic.sType(), optic.tType()));
    }

    @Override
    public String toString(final int level) {
        return "Optic[" + optic + "]";
    }

    @Override
    public Function<DynamicOps<?>, Function<Function<A, B>, Function<S, T>>> eval() {
        final Function<App2<FunctionType.Mu, A, B>, App2<FunctionType.Mu, S, T>> func = optic.upCast(FunctionType.Instance.Mu.TYPE_TOKEN).orElseThrow().eval(FunctionType.Instance.INSTANCE);
        final Function<Function<A, B>, Function<S, T>> unwrappedFunction = input -> FunctionType.unbox(func.apply(FunctionType.create(input)));
        return ops -> unwrappedFunction;
    }

    @Override
    public boolean equals(final Object o) {
        if (this == o) {
            return true;
        }
        if (o == null || getClass() != o.getClass()) {
            return false;
        }
        final ProfunctorTransformer<?, ?, ?, ?> that = (ProfunctorTransformer<?, ?, ?, ?>) o;
        return Objects.equals(optic, that.optic);
    }

    @Override
    public int hashCode() {
        return optic.hashCode();
    }
}

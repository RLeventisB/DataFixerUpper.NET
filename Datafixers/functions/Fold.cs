// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers.functions;

using com.google.common.collect.Maps;
using DataFixerUpper.Datafixers.DSL;
using DataFixerUpper.Datafixers.RewriteResult;
using DataFixerUpper.Datafixers.View;
using DataFixerUpper.Datafixers.types.Type;
using DataFixerUpper.Datafixers.types.families.Algebra;
using DataFixerUpper.Datafixers.types.families.ListAlgebra;
using DataFixerUpper.Datafixers.types.families.RecursiveTypeFamily;
using DataFixerUpper.Datafixers.types.templates.RecursivePoint;
using DataFixerUpper.Datafixers.util.Pair;
using DataFixerUpper.Serialization.DynamicOps;

using java.util.ArrayList;
using java.util.List;
using java.util.Map;
using java.util.Objects;
using java.util.Optional;
using java.util.function.Function;
using java.util.function.IntFunction;

final class Fold<A, B> extends PointFree<Function<A, B>> {
    private static final Map<HmapCacheKey, IntFunction<RewriteResult<?, ?>>> HMAP_CACHE = Maps.newConcurrentMap();
    private static final Map<Pair<IntFunction<RewriteResult<?, ?>>, Integer>, RewriteResult<?, ?>> HMAP_APPLY_CACHE = Maps.newConcurrentMap();

    private record HmapCacheKey(RecursiveTypeFamily family, RecursiveTypeFamily newFamily, Algebra algebra) {
    }

    protected final RecursivePoint.RecursivePointType<A> aType;
    protected final RecursivePoint.RecursivePointType<B> bType;
    protected final Algebra algebra;
    protected final int index;

    public Fold(final RecursivePoint.RecursivePointType<A> aType, final RecursivePoint.RecursivePointType<B> bType, final Algebra algebra, final int index) {
        this.aType = aType;
        this.bType = bType;
        this.algebra = algebra;
        this.index = index;
    }

    @Override
    public Type<Function<A, B>> type() {
        return DSL.func(aType, bType);
    }

    @Override
    Optional<? extends PointFree<Function<A, B>>> all(final PointFreeRule rule) {
        final int familySize = aType.family().size();
        final List<RewriteResult<?, ?>> newAlgebra = new ArrayList<>(familySize);
        boolean changed = false;
        for (int i = 0; i < familySize; i++) {
            final RewriteResult<?, ?> view = algebra.apply(i);
            final PointFree<? extends Function<?, ?>> function = view.view().function();
            final PointFree<? extends Function<?, ?>> rewrite = rule.rewriteOrNop(function);
            if (rewrite != function) {
                newAlgebra.add(cap(view, rewrite));
                changed = true;
            } else {
                newAlgebra.add(view);
            }
        }
        if (changed) {
            return Optional.of(new Fold<>(aType, bType, new ListAlgebra("Rewrite all", newAlgebra), index));
        }
        return Optional.empty();
    }

    @SuppressWarnings("unchecked")
    private static <A, B> RewriteResult<A, B> cap(final RewriteResult<A, B> view, final PointFree<? extends Function<?, ?>> rewrite) {
        return RewriteResult.create(new View<>((PointFree<Function<A, B>>) rewrite), view.recData());
    }

    private <FB> PointFree<Function<A, B>> cap(final RewriteResult<?, FB> resResult) {
        final RewriteResult<A, B> op = (RewriteResult<A, B>) algebra.apply(index);
        return Functions.comp(((View<FB, B>) op.view()).function(), ((View<A, FB>) resResult.view()).function());
    }

    @Override
    public Function<DynamicOps<?>, Function<A, B>> eval() {
        return ops -> a -> {
            final RecursiveTypeFamily family = aType.family();
            final RecursiveTypeFamily newFamily = bType.family();

            final IntFunction<RewriteResult<?, ?>> hmapped = HMAP_CACHE.computeIfAbsent(new HmapCacheKey(family, newFamily, algebra), key -> key.family().template().hmap(key.family(), key.family().fold(key.algebra(), key.newFamily())));
            final RewriteResult<?, ?> result = HMAP_APPLY_CACHE.computeIfAbsent(Pair.of(hmapped, index), key -> key.getFirst().apply(key.getSecond()));

            final PointFree<Function<A, B>> eval = cap(result);
            return eval.evalCached().apply(ops).apply(a);
        };
    }

    @Override
    public String toString(final int level) {
        return "fold(" + aType + ", " + index + ", \n" + indent(level + 1) + algebra.toString(level + 1) + "\n" + indent(level) + ")";
    }

    @Override
    public boolean equals(final Object o) {
        if (this == o) {
            return true;
        }
        if (o == null || getClass() != o.getClass()) {
            return false;
        }
        final Fold<?, ?> fold = (Fold<?, ?>) o;
        return Objects.equals(aType, fold.aType) && Objects.equals(bType, fold.bType) && Objects.equals(algebra, fold.algebra);
    }

    @Override
    public int hashCode() {
        int result = aType.hashCode();
        result = 31 * result + bType.hashCode();
        result = 31 * result + algebra.hashCode();
        return result;
    }
}

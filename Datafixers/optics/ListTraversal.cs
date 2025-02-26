// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers.optics;

using com.google.common.collect.ImmutableList;
using DataFixerUpper.Datafixers.FunctionType;
using DataFixerUpper.Datafixers.kinds.App;
using DataFixerUpper.Datafixers.kinds.Applicative;
using DataFixerUpper.Datafixers.kinds.K1;

using java.util.List;

public final class ListTraversal<A, B> implements Traversal<List<A>, List<B>, A, B> {
    static final ListTraversal<?, ?> INSTANCE = new ListTraversal<>();

    private ListTraversal() {
    }

    @Override
    public <F extends K1> FunctionType<List<A>, App<F, List<B>>> wander(final Applicative<F, ?> applicative, final FunctionType<A, App<F, B>> input) {
        return as -> {
            App<F, ImmutableList.Builder<B>> result = applicative.point(ImmutableList.builder());
            for (final A a : as) {
                result = applicative.ap2(applicative.point(ImmutableList.Builder::add), result, input.apply(a));
            }
            return applicative.map(ImmutableList.Builder::build, result);
        };
    }

    @Override
    public String toString() {
        return "ListTraversal";
    }
}

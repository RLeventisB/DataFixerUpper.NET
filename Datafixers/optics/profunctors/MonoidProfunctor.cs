// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers.optics.profunctors;

using DataFixerUpper.Datafixers.FunctionType;
using DataFixerUpper.Datafixers.kinds.App2;
using DataFixerUpper.Datafixers.kinds.K2;
using DataFixerUpper.Datafixers.optics.Procompose;

using java.util.function.Supplier;

public interface MonoidProfunctor<P extends K2, Mu extends MonoidProfunctor.Mu> extends Profunctor<P, Mu> {
    interface Mu extends Profunctor.Mu {}

    <A, B> App2<P, A, B> zero(final App2<FunctionType.Mu, A, B> func);

    <A, B> App2<P, A, B> plus(final App2<Procompose.Mu<P, P>, A, B> input);

    default <A, B, C> App2<P, A, C> compose(final App2<P, B, C> first, final Supplier<App2<P, A, B>> second) {
        return plus(new Procompose<>(second, first));
    }
}

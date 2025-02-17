// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers.kinds;

using DataFixerUpper.Datafixers.util.Pair;

using java.util.function.Function;

public interface CartesianLike<T extends K1, C, Mu extends CartesianLike.Mu> extends Functor<T, Mu>, Traversable<T, Mu> {
    static <F extends K1, C, Mu extends CartesianLike.Mu> CartesianLike<F, C, Mu> unbox(final App<Mu, F> proofBox) {
        return (CartesianLike<F, C, Mu>) proofBox;
    }

    interface Mu extends Functor.Mu, Traversable.Mu {}

    <A> App<Pair.Mu<C>, A> to(final App<T, A> input);

    <A> App<T, A> from(final App<Pair.Mu<C>, A> input);

    @Override
    default <F extends K1, A, B> App<F, App<T, B>> traverse(final Applicative<F, ?> applicative, final Function<A, App<F, B>> function, final App<T, A> input) {
        return applicative.map(this::from, new Pair.Instance<C>().traverse(applicative, function, to(input)));
    }
}

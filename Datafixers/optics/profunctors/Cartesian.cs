// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers.optics.profunctors;

using com.google.common.reflect.TypeToken;
using DataFixerUpper.Datafixers.kinds.App;
using DataFixerUpper.Datafixers.kinds.App2;
using DataFixerUpper.Datafixers.kinds.CartesianLike;
using DataFixerUpper.Datafixers.kinds.K1;
using DataFixerUpper.Datafixers.kinds.K2;
using DataFixerUpper.Datafixers.util.Pair;

public interface Cartesian<P extends K2, Mu extends Cartesian.Mu> extends Profunctor<P, Mu> {
    static <P extends K2, Proof extends Cartesian.Mu> Cartesian<P, Proof> unbox(final App<Proof, P> proofBox) {
        return (Cartesian<P, Proof>) proofBox;
    }

    interface Mu extends Profunctor.Mu {
        TypeToken<Mu> TYPE_TOKEN = new TypeToken<Mu>() {};
    }

    <A, B, C> App2<P, Pair<A, C>, Pair<B, C>> first(final App2<P, A, B> input);

    default <A, B, C> App2<P, Pair<C, A>, Pair<C, B>> second(final App2<P, A, B> input) {
        return dimap(first(input), Pair::swap, Pair::swap);
    }

    default FunctorProfunctor<CartesianLike.Mu, P, FunctorProfunctor.Mu<CartesianLike.Mu>> toFP2() {
        return new FunctorProfunctor<CartesianLike.Mu, P, FunctorProfunctor.Mu<CartesianLike.Mu>>() {
            @Override
            public <A, B, F extends K1> App2<P, App<F, A>, App<F, B>> distribute(final App<? extends CartesianLike.Mu, F> proof, final App2<P, A, B> input) {
                return cap(CartesianLike.unbox(proof), input);
            }

            private <A, B, F extends K1, C> App2<P, App<F, A>, App<F, B>> cap(final CartesianLike<F, C, ?> cLike, final App2<P, A, B> input) {
                return dimap(first(input), p -> Pair.unbox(cLike.to(p)), cLike::from);
            }
        };
    }
}

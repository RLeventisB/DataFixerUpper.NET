// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers.optics.profunctors;

using com.google.common.reflect.TypeToken;
using DataFixerUpper.Datafixers.FunctionType;
using DataFixerUpper.Datafixers.kinds.App;
using DataFixerUpper.Datafixers.kinds.App2;
using DataFixerUpper.Datafixers.kinds.Applicative;
using DataFixerUpper.Datafixers.kinds.K1;
using DataFixerUpper.Datafixers.kinds.K2;
using DataFixerUpper.Datafixers.kinds.Traversable;
using DataFixerUpper.Datafixers.optics.Wander;
using DataFixerUpper.Datafixers.util.Either;
using DataFixerUpper.Datafixers.util.Pair;

public interface TraversalP<P extends K2, Mu extends TraversalP.Mu> extends AffineP<P, Mu>/*, Monoidal<P, Mu>*/ {
    static <P extends K2, Proof extends TraversalP.Mu> TraversalP<P, Proof> unbox(final App<Proof, P> proofBox) {
        return (TraversalP<P, Proof>) proofBox;
    }

    public interface Mu extends AffineP.Mu/*, Monoidal.Mu*/ {
        TypeToken<Mu> TYPE_TOKEN = new TypeToken<Mu>() {};
    }

    <S, T, A, B> App2<P, S, T> wander(final Wander<S, T, A, B> wander, final App2<P, A, B> input);

    /*default <S, T, A, B> App2<P, S, T> wander(final Wander<S, T, A, B> wander, final App2<P, A, B> input) {
        return this.<App<Baz.Mu<T, B>, A>, App<Baz.Mu<T, B>, B>, S, T>dimap(
            traverse(new Baz.Instance<>(), input),
            s -> new Baz<T, B, A>(){
                @Override
                public <F extends K1> App<F, T> run(final Applicative<F> applicative, final FunctionType<A, App<F, B>> function) {
                    return wander.<F>wander(applicative, function).apply(s);
                }
            },
            Baz::<T, B>sold
        );
    }*/

    /*@Override
    default <A, B, F extends K1> App2<P, App<F, A>, App<F, B>> distribute(final App<T, F> proof, final App2<P, A, B> input) {
        return traverse(Traversable.unbox(proof), input);
    }*/

    default <T extends K1, A, B> App2<P, App<T, A>, App<T, B>> traverse(final Traversable<T, ?> traversable, final App2<P, A, B> input) {
        return wander(new Wander<App<T, A>, App<T, B>, A, B>() {
            @Override
            public <F extends K1> FunctionType<App<T, A>, App<F, App<T, B>>> wander(final Applicative<F, ?> applicative, final FunctionType<A, App<F, B>> function) {
                return ta -> traversable.<F, A, B>traverse(applicative, function, ta);
            }
        }, input);
    }

    @Override
    default <A, B, C> App2<P, Pair<A, C>, Pair<B, C>> first(final App2<P, A, B> input) {
        return dimap(traverse(new Pair.Instance<>(), input), box -> box, Pair::unbox);
    }

    @Override
    default <A, B, C> App2<P, Either<A, C>, Either<B, C>> left(final App2<P, A, B> input) {
        return dimap(traverse(new Either.Instance<>(), input), box -> box, Either::unbox);
    }

    default FunctorProfunctor<Traversable.Mu, P, FunctorProfunctor.Mu<Traversable.Mu>> toFP3() {
        return new FunctorProfunctor<Traversable.Mu, P, FunctorProfunctor.Mu<Traversable.Mu>>() {
            @Override
            public <A, B, F extends K1> App2<P, App<F, A>, App<F, B>> distribute(final App<? extends Traversable.Mu, F> proof, final App2<P, A, B> input) {
                return traverse(Traversable.unbox(proof), input);
            }
        };
    }
}

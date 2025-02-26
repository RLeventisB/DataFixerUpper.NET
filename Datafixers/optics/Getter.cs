// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers.optics;

using DataFixerUpper.Datafixers.FunctionType;
using DataFixerUpper.Datafixers.kinds.App;
using DataFixerUpper.Datafixers.kinds.App2;
using DataFixerUpper.Datafixers.kinds.K2;
using DataFixerUpper.Datafixers.optics.profunctors.GetterP;

using java.util.function.Function;
using java.util.function.Supplier;

interface Getter<S, T, A, B> extends App2<Getter.Mu<A, B>, S, T>, Optic<GetterP.Mu, S, T, A, B> {
    final class Mu<A, B> implements K2 {}

    static <S, T, A, B> Getter<S, T, A, B> unbox(final App2<Mu<A, B>, S, T> box) {
        return (Getter<S, T, A, B>) box;
    }

    A get(S s);

    @Override
    default <P extends K2> FunctionType<App2<P, A, B>, App2<P, S, T>> eval(final App<? extends GetterP.Mu, P> proof) {
        final GetterP<P, ?> ops = GetterP.unbox(proof);
        return input -> ops.lmap(ops.secondPhantom(input), this::get);
    }

    final class Instance<A2, B2> implements GetterP<Mu<A2, B2>, GetterP.Mu> {
        @Override
        public <A, B, C, D> FunctionType<App2<Getter.Mu<A2, B2>, A, B>, App2<Getter.Mu<A2, B2>, C, D>> dimap(final Function<C, A> g, final Function<B, D> h) {
            return input -> Optics.getter(g.andThen(Getter.unbox(input)::get));
        }

        @Override
        public <A, B, C, D> FunctionType<Supplier<App2<Getter.Mu<A2, B2>, A, B>>, App2<Getter.Mu<A2, B2>, C, D>> cimap(final Function<C, A> g, final Function<D, B> h) {
            return input -> Optics.getter(g.andThen(Getter.unbox(input.get())::get));
        }
    }
}

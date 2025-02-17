// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers.optics;

using DataFixerUpper.Datafixers.FunctionType;
using DataFixerUpper.Datafixers.kinds.App;
using DataFixerUpper.Datafixers.kinds.App2;
using DataFixerUpper.Datafixers.kinds.K2;
using DataFixerUpper.Datafixers.optics.profunctors.Cartesian;
using DataFixerUpper.Datafixers.optics.profunctors.ReCocartesian;
using DataFixerUpper.Datafixers.util.Either;
using DataFixerUpper.Datafixers.util.Pair;

using java.util.function.Function;

public interface Forget<R, A, B> extends App2<Forget.Mu<R>, A, B> {
    final class Mu<R> implements K2 {}

    static <R, A, B> Forget<R, A, B> unbox(final App2<Mu<R>, A, B> box) {
        return (Forget<R, A, B>) box;
    }

    R run(final A a);

    final class Instance<R> implements Cartesian<Mu<R>, Instance.Mu<R>>, ReCocartesian<Mu<R>, Instance.Mu<R>>, App<Instance.Mu<R>, Mu<R>> {
        public static final class Mu<R> implements Cartesian.Mu, ReCocartesian.Mu {}

        @Override
        public <A, B, C, D> FunctionType<App2<Forget.Mu<R>, A, B>, App2<Forget.Mu<R>, C, D>> dimap(final Function<C, A> g, final Function<B, D> h) {
            return input -> Optics.forget(c -> Forget.unbox(input).run(g.apply(c)));
        }

        @Override
        public <A, B, C> App2<Forget.Mu<R>, Pair<A, C>, Pair<B, C>> first(final App2<Forget.Mu<R>, A, B> input) {
            return Optics.forget(p -> Forget.unbox(input).run(p.getFirst()));
        }

        @Override
        public <A, B, C> App2<Forget.Mu<R>, Pair<C, A>, Pair<C, B>> second(final App2<Forget.Mu<R>, A, B> input) {
            return Optics.forget(p -> Forget.unbox(input).run(p.getSecond()));
        }

        @Override
        public <A, B, C> App2<Forget.Mu<R>, A, B> unleft(final App2<Forget.Mu<R>, Either<A, C>, Either<B, C>> input) {
            return Optics.forget(a -> Forget.unbox(input).run(Either.left(a)));
        }

        @Override
        public <A, B, C> App2<Forget.Mu<R>, A, B> unright(final App2<Forget.Mu<R>, Either<C, A>, Either<C, B>> input) {
            return Optics.forget(a -> Forget.unbox(input).run(Either.right(a)));
        }
    }
}

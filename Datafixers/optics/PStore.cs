// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers.optics;

using DataFixerUpper.Datafixers.kinds.App;
using DataFixerUpper.Datafixers.kinds.Functor;
using DataFixerUpper.Datafixers.kinds.K1;

using java.util.function.Function;

interface PStore<I, J, X> extends App<PStore.Mu<I, J>, X> {
    final class Mu<I, J> implements K1 {}

    static <I, J, X> PStore<I, J, X> unbox(final App<Mu<I, J>, X> box) {
        return (PStore<I, J, X>) box;
    }

    X peek(final J j);

    I pos();

    final class Instance<I, J> implements Functor<Mu<I, J>, Instance.Mu<I, J>> {
        public static final class Mu<I, J> implements Functor.Mu {}

        @Override
        public <T, R> App<PStore.Mu<I, J>, R> map(final Function<? super T, ? extends R> func, final App<PStore.Mu<I, J>, T> ts) {
            final PStore<I, J, T> input = PStore.unbox(ts);
            return Optics.pStore(func.compose(input::peek)::apply, input::pos);
        }
    }
}

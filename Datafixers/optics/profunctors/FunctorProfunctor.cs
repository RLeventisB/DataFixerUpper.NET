// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers.optics.profunctors;

using DataFixerUpper.Datafixers.kinds.App;
using DataFixerUpper.Datafixers.kinds.App2;
using DataFixerUpper.Datafixers.kinds.K1;
using DataFixerUpper.Datafixers.kinds.K2;
using DataFixerUpper.Datafixers.kinds.Kind2;

public interface FunctorProfunctor<T extends K1, P extends K2, Mu extends FunctorProfunctor.Mu<T>> extends Kind2<P, Mu> {
    static <T extends K1, P extends K2, Mu extends FunctorProfunctor.Mu<T>> FunctorProfunctor<T, P, Mu> unbox(final App<Mu, P> proofBox) {
        return (FunctorProfunctor<T, P, Mu>) proofBox;
    }

    interface Mu<T extends K1> extends Kind2.Mu {}

    <A, B, F extends K1> App2<P, App<F, A>, App<F, B>> distribute(final App<? extends T, F> proof, final App2<P, A, B> input);
}

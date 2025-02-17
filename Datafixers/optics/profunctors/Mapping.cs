// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers.optics.profunctors;

using DataFixerUpper.Datafixers.kinds.App;
using DataFixerUpper.Datafixers.kinds.App2;
using DataFixerUpper.Datafixers.kinds.Functor;
using DataFixerUpper.Datafixers.kinds.K1;
using DataFixerUpper.Datafixers.kinds.K2;

public interface Mapping<P extends K2, Mu extends Mapping.Mu> extends TraversalP<P, Mu> {
    static <P extends K2, Proof extends Mapping.Mu> Mapping<P, Proof> unbox(final App<Proof, P> proofBox) {
        return (Mapping<P, Proof>) proofBox;
    }

    interface Mu extends TraversalP.Mu {}

    <A, B, F extends K1> App2<P, App<F, A>, App<F, B>> mapping(final Functor<F, ?> functor, final App2<P, A, B> input);
}

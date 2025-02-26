// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers.optics.profunctors;

using DataFixerUpper.Datafixers.kinds.App;
using DataFixerUpper.Datafixers.kinds.App2;
using DataFixerUpper.Datafixers.kinds.K2;

using java.util.function.Function;

public interface GetterP<P extends K2, Mu extends GetterP.Mu> extends Profunctor<P, Mu>, Bicontravariant<P, Mu> {
    static <P extends K2, Proof extends GetterP.Mu> GetterP<P, Proof> unbox(final App<Proof, P> proofBox) {
        return (GetterP<P, Proof>) proofBox;
    }

    interface Mu extends Profunctor.Mu, Bicontravariant.Mu {}

    default <A, B, C> App2<P, C, A> secondPhantom(final App2<P, C, B> input) {
        return cimap(() -> rmap(input, b -> (Void) null), Function.identity(), a -> null);
    }
}

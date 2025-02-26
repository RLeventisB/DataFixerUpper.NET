// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers;

using DataFixerUpper.Datafixers.types.Type;
using DataFixerUpper.Datafixers.types.templates.RecursivePoint;

using java.util.BitSet;
using java.util.Objects;

public record RewriteResult<A, B>(View<A, B> view, BitSet recData) {
    public static <A, B> RewriteResult<A, B> create(final View<A, B> view, final BitSet recData) {
        return new RewriteResult<>(view, recData);
    }

    public static <A> RewriteResult<A, A> nop(final Type<A> type) {
        return new RewriteResult<>(View.nopView(type), new BitSet());
    }

    public <C> RewriteResult<C, B> compose(final RewriteResult<C, A> that) {
        final BitSet newData;
        if (view.type() instanceof RecursivePoint.RecursivePointType<?> && that.view.type() instanceof RecursivePoint.RecursivePointType<?>) {
            // same family, merge results - not exactly accurate, but should be good enough
            newData = (BitSet) recData.clone();
            newData.or(that.recData);
        } else {
            newData = recData;
        }
        return create(view.compose(that.view), newData);
    }

    @Override
    public String toString() {
        return "RR[" + view + "]";
    }

    @Override
    public boolean equals(final Object o) {
        if (this == o) {
            return true;
        }
        if (o == null || getClass() != o.getClass()) {
            return false;
        }
        final RewriteResult<?, ?> that = (RewriteResult<?, ?>) o;
        return Objects.equals(view, that.view);
    }

    @Override
    public int hashCode() {
        return view.hashCode();
    }
}

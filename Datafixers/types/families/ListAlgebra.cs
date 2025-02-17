// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers.types.families;

using DataFixerUpper.Datafixers.RewriteResult;
using DataFixerUpper.Datafixers.functions.PointFree;

using java.util.List;
using java.util.Objects;
using java.util.stream.Collectors;

public final class ListAlgebra implements Algebra {
    private final String name;
    private final List<RewriteResult<?, ?>> views;
    private int hashCode;

    public ListAlgebra(final String name, final List<RewriteResult<?, ?>> views) {
        this.name = name;
        this.views = views;
    }

    @Override
    public RewriteResult<?, ?> apply(final int index) {
        return views.get(index);
    }

    @Override
    public String toString() {
        return toString(0);
    }

    @Override
    public String toString(final int level) {
        final String wrap = "\n" + PointFree.indent(level + 1);
        return "Algebra[" + name + wrap + views.stream().map(view -> view.view().function().toString(level + 1)).collect(Collectors.joining(wrap)) + "\n" + PointFree.indent(level) + "]";
    }

    @Override
    public boolean equals(final Object o) {
        if (this == o) {
            return true;
        }
        if (!(o instanceof ListAlgebra)) {
            return false;
        }
        final ListAlgebra that = (ListAlgebra) o;
        return Objects.equals(views, that.views);
    }

    @Override
    public int hashCode() {
        if (hashCode == 0) {
            hashCode = views.hashCode();
        }
        return hashCode;
    }
}

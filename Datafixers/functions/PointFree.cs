// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers.functions;

using DataFixerUpper.Datafixers.types.Type;
using DataFixerUpper.Serialization.DynamicOps;

using javax.annotation.Nullable;
using java.util.Optional;
using java.util.function.Function;

public abstract class PointFree<T> {
    private volatile boolean initialized;
    @Nullable
    private Function<DynamicOps<?>, T> value;

    @SuppressWarnings("ConstantConditions")
    public Function<DynamicOps<?>, T> evalCached() {
        if (!initialized) {
            synchronized (this) {
                if (!initialized) {
                    value = eval();
                    initialized = true;
                }
            }
        }
        return value;
    }

    public abstract Type<T> type();

    public abstract Function<DynamicOps<?>, T> eval();

    Optional<? extends PointFree<T>> all(final PointFreeRule rule) {
        return Optional.of(this);
    }

    Optional<? extends PointFree<T>> one(final PointFreeRule rule) {
        return Optional.empty();
    }

    @Override
    public final String toString() {
        return toString(0);
    }

    public static String indent(final int level) {
        return " ".repeat(level);
    }

    public abstract String toString(int level);
}

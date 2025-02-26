// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers;

using com.google.common.collect.ImmutableList;
using com.google.common.reflect.TypeToken;
using DataFixerUpper.Datafixers.kinds.Const;
using DataFixerUpper.Datafixers.kinds.IdF;
using DataFixerUpper.Datafixers.kinds.Monoid;
using DataFixerUpper.Datafixers.optics.Forget;
using DataFixerUpper.Datafixers.optics.ForgetOpt;
using DataFixerUpper.Datafixers.optics.Inj1;
using DataFixerUpper.Datafixers.optics.Inj2;
using DataFixerUpper.Datafixers.optics.Optics;
using DataFixerUpper.Datafixers.optics.ReForgetC;
using DataFixerUpper.Datafixers.optics.Traversal;
using DataFixerUpper.Datafixers.optics.profunctors.TraversalP;
using DataFixerUpper.Datafixers.types.Type;
using DataFixerUpper.Datafixers.types.templates.RecursivePoint;
using DataFixerUpper.Datafixers.util.Either;
using DataFixerUpper.Datafixers.util.Pair;
using DataFixerUpper.Serialization.DataResult;
using DataFixerUpper.Serialization.Dynamic;
using DataFixerUpper.Serialization.DynamicOps;

using java.util.List;
using java.util.Optional;
using java.util.function.Function;
using java.util.stream.Collectors;

public final class Typed<A> {
    protected final Type<A> type;
    protected final DynamicOps<?> ops;
    protected final A value;

    public Typed(final Type<A> type, final DynamicOps<?> ops, final A value) {
        this.type = type;
        this.ops = ops;
        this.value = value;
    }

    @Override
    public String toString() {
        return "Typed[" + value + "]";
    }

    public <FT> FT get(final OpticFinder<FT> optic) {
        return Forget.unbox(optic.findType(type, false).orThrow().apply(
            new TypeToken<Forget.Instance.Mu<FT>>() {},
            new Forget.Instance<>(),
            Optics.forget(Function.identity())
        )).run(value);
    }

    public <FT> Typed<FT> getTyped(final OpticFinder<FT> optic) {
        final TypedOptic<A, ?, FT, FT> o = optic.findType(type, false).orThrow();
        return new Typed<>(o.aType(), ops, Forget.unbox(o.apply(
            new TypeToken<Forget.Instance.Mu<FT>>() {},
            new Forget.Instance<>(),
            Optics.forget(Function.identity())
        )).run(value));
    }

    public <FT> Optional<FT> getOptional(final OpticFinder<FT> optic) {
        final TypedOptic<A, ?, FT, FT> optic1 = optic.findType(type, false).orThrow();
        return ForgetOpt.unbox(optic1.apply(
            new TypeToken<ForgetOpt.Instance.Mu<FT>>() {},
            new ForgetOpt.Instance<>(),
            Optics.forgetOpt(Optional::of))
        ).run(value);
    }

    public <FT> FT getOrCreate(final OpticFinder<FT> optic) {
        return DataFixUtils.or(getOptional(optic), () -> optic.type().point(ops)).orElseThrow(() -> new IllegalStateException("Could not create default value for type: " + optic.type()));
    }

    public <FT> FT getOrDefault(final OpticFinder<FT> optic, final FT def) {
        return ForgetOpt.unbox(optic.findType(type, false).orThrow().apply(
            new TypeToken<ForgetOpt.Instance.Mu<FT>>() {},
            new ForgetOpt.Instance<>(),
            Optics.forgetOpt(Optional::of))
        ).run(value).orElse(def);
    }

    public <FT> Optional<Typed<FT>> getOptionalTyped(final OpticFinder<FT> optic) {
        final TypedOptic<A, ?, FT, FT> o = optic.findType(type, false).orThrow();
        return ForgetOpt.unbox(o.apply(
            new TypeToken<ForgetOpt.Instance.Mu<FT>>() {},
            new ForgetOpt.Instance<>(),
            Optics.forgetOpt(Optional::of))
        ).run(value).map(v -> new Typed<>(o.aType(), ops, v));
    }

    public <FT> Typed<FT> getOrCreateTyped(final OpticFinder<FT> optic) {
        return DataFixUtils.or(getOptionalTyped(optic), () -> optic.type().pointTyped(ops)).orElseThrow(() -> new IllegalStateException("Could not create default value for type: " + optic.type()));
    }

    public <FT> Typed<?> set(final OpticFinder<FT> optic, final FT newValue) {
        return set(optic, new Typed<>(optic.type(), ops, newValue));
    }

    public <FT, FR> Typed<?> set(final OpticFinder<FT> optic, final Type<FR> newType, final FR newValue) {
        return set(optic, new Typed<>(newType, ops, newValue));
    }

    public <FT, FR> Typed<?> set(final OpticFinder<FT> optic, final Typed<FR> newValue) {
        final TypedOptic<A, ?, FT, FR> field = optic.findType(type, newValue.type, false).orThrow();
        return setCap(field, newValue);
    }

    private <B, FT, FR> Typed<B> setCap(final TypedOptic<A, B, FT, FR> field, final Typed<FR> newValue) {
        final B b = ReForgetC.unbox(field.apply(
            new TypeToken<ReForgetC.Instance.Mu<FR>>() {},
            new ReForgetC.Instance<>(),
            Optics.reForgetC("set", Either.left(Function.identity())))
        ).run(value, newValue.value);
        return new Typed<>(field.tType(), ops, b);
    }

    public <FT> Typed<?> updateTyped(final OpticFinder<FT> optic, final Function<Typed<?>, Typed<?>> updater) {
        return updateTyped(optic, optic.type(), updater);
    }

    public <FT, FR> Typed<?> updateTyped(final OpticFinder<FT> optic, final Type<FR> newType, final Function<Typed<?>, Typed<?>> updater) {
        final TypedOptic<A, ?, FT, FR> field = optic.findType(type, newType, false).orThrow();
        return updateCap(field, ft -> {
            final Typed<?> newValue = updater.apply(new Typed<>(optic.type(), ops, ft));
            return field.bType().ifSame(newValue).orElseThrow(() -> new IllegalArgumentException("Function didn't update to the expected type"));
        });
    }

    public <FT> Typed<?> update(final OpticFinder<FT> optic, final Function<FT, FT> updater) {
        return update(optic, optic.type(), updater);
    }

    public <FT, FR> Typed<?> update(final OpticFinder<FT> optic, final Type<FR> newType, final Function<FT, FR> updater) {
        final TypedOptic<A, ?, FT, FR> field = optic.findType(type, newType, false).orThrow();
        return updateCap(field, updater);
    }

    public <FT> Typed<?> updateRecursiveTyped(final OpticFinder<FT> optic, final Function<Typed<?>, Typed<?>> updater) {
        return updateRecursiveTyped(optic, optic.type(), updater);
    }

    public <FT, FR> Typed<?> updateRecursiveTyped(final OpticFinder<FT> optic, final Type<FR> newType, final Function<Typed<?>, Typed<?>> updater) {
        final TypedOptic<A, ?, FT, FR> field = optic.findType(type, newType, true).orThrow();
        return updateCap(field, ft -> {
            final Typed<?> newValue = updater.apply(new Typed<>(optic.type(), ops, ft));
            return field.bType().ifSame(newValue).orElseThrow(() -> new IllegalArgumentException("Function didn't update to the expected type"));
        });
    }

    public <FT> Typed<?> updateRecursive(final OpticFinder<FT> optic, final Function<FT, FT> updater) {
        return updateRecursive(optic, optic.type(), updater);
    }

    public <FT, FR> Typed<?> updateRecursive(final OpticFinder<FT> optic, final Type<FR> newType, final Function<FT, FR> updater) {
        final TypedOptic<A, ?, FT, FR> field = optic.findType(type, newType, true).orThrow();
        return updateCap(field, updater);
    }

    private <B, FT, FR> Typed<B> updateCap(final TypedOptic<A, B, FT, FR> field, final Function<FT, FR> updater) {
        final Traversal<A, B, FT, FR> traversal = Optics.toTraversal(field.upCast(TraversalP.Mu.TYPE_TOKEN).orElseThrow(IllegalArgumentException::new));
        final B b = IdF.get(traversal.wander(IdF.Instance.INSTANCE, ft -> IdF.create(updater.apply(ft))).apply(value));
        return new Typed<>(field.tType(), ops, b);
    }

    public <FT> List<Typed<FT>> getAllTyped(final OpticFinder<FT> optic) {
        final TypedOptic<A, ?, FT, ?> field = optic.findType(type, optic.type(), false).orThrow();
        return getAll(field).stream().map(ft -> new Typed<>(optic.type(), ops, ft)).collect(Collectors.toList());
    }

    public <FT> List<FT> getAll(final TypedOptic<A, ?, FT, ?> field) {
        final Traversal<A, ?, FT, ?> traversal = Optics.toTraversal(field.upCast(TraversalP.Mu.TYPE_TOKEN).orElseThrow(IllegalArgumentException::new));
        return Const.unbox(traversal.wander(new Const.Instance<>(Monoid.listMonoid()), ft -> Const.create(ImmutableList.of(ft))).apply(value));
    }

    public Typed<A> out() {
        if (!(type instanceof RecursivePoint.RecursivePointType<?>)) {
            throw new IllegalArgumentException("Not recursive");
        }
        final Type<A> unfold = ((RecursivePoint.RecursivePointType<A>) type).unfold();
        return new Typed<>(unfold, ops, value);
    }

    public <B> Typed<Either<A, B>> inj1(final Type<B> type) {
        return new Typed<>(DSL.or(this.type, type), ops, Optics.<A, B, A>inj1().build(value));
    }

    public <B> Typed<Either<B, A>> inj2(final Type<B> type) {
        return new Typed<>(DSL.or(type, this.type), ops, Optics.<B, A, A>inj2().build(value));
    }

    public static <A, B> Typed<Pair<A, B>> pair(final Typed<A> first, final Typed<B> second) {
        return new Typed<>(DSL.and(first.type, second.type), first.ops, Pair.of(first.value, second.value));
    }

    public Type<A> getType() {
        return type;
    }

    public DynamicOps<?> getOps() {
        return ops;
    }

    public A getValue() {
        return value;
    }

    public DataResult<? extends Dynamic<?>> write() {
        return type.writeDynamic(ops, value);
    }
}

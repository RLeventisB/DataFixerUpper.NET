// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace DataFixerUpper.Datafixers.types.templates;

using DataFixerUpper.Datafixers.DSL;
using DataFixerUpper.Datafixers.FamilyOptic;
using DataFixerUpper.Datafixers.RewriteResult;
using DataFixerUpper.Datafixers.TypeRewriteRule;
using DataFixerUpper.Datafixers.TypedOptic;
using DataFixerUpper.Datafixers.optics.Optics;
using DataFixerUpper.Datafixers.optics.profunctors.Cartesian;
using DataFixerUpper.Datafixers.types.Type;
using DataFixerUpper.Datafixers.types.families.RecursiveTypeFamily;
using DataFixerUpper.Datafixers.types.families.TypeFamily;
using DataFixerUpper.Datafixers.util.Either;
using DataFixerUpper.Datafixers.util.Pair;
using DataFixerUpper.Serialization.Codec;
using DataFixerUpper.Serialization.DataResult;
using DataFixerUpper.Serialization.DynamicOps;
using DataFixerUpper.Serialization.Lifecycle;

using javax.annotation.Nullable;
using java.util.Objects;
using java.util.Optional;
using java.util.function.IntFunction;

public record Named(String name, TypeTemplate element) implements TypeTemplate {
    @Override
    public int size() {
        return element.size();
    }

    @Override
    public TypeFamily apply(final TypeFamily family) {
        return index -> DSL.named(name, element.apply(family).apply(index));
    }

    @Override
    public <A, B> FamilyOptic<A, B> applyO(final FamilyOptic<A, B> input, final Type<A> aType, final Type<B> bType) {
        return TypeFamily.familyOptic(i -> element.applyO(input, aType, bType).apply(i));
    }

    @Override
    public <FT, FR> Either<TypeTemplate, Type.FieldNotFoundException> findFieldOrType(final int index, @Nullable final String name, final Type<FT> type, final Type<FR> resultType) {
        return element.findFieldOrType(index, name, type, resultType);
    }

    @Override
    public IntFunction<RewriteResult<?, ?>> hmap(final TypeFamily family, final IntFunction<RewriteResult<?, ?>> function) {
        return index -> {
            final RewriteResult<?, ?> elementResult = element.hmap(family, function).apply(index);
            return cap(family, index, elementResult);
        };
    }

    private <A> RewriteResult<Pair<String, A>, ?> cap(final TypeFamily family, final int index, final RewriteResult<A, ?> elementResult) {
        return NamedType.fix((NamedType<A>) apply(family).apply(index), elementResult);
    }

    @Override
    public String toString() {
        return "NamedTypeTag[" + name + ": " + element + "]";
    }

    public static final class NamedType<A> extends Type<Pair<String, A>> {
        protected final String name;
        protected final Type<A> element;

        public NamedType(final String name, final Type<A> element) {
            this.name = name;
            this.element = element;
        }

        public static <A, B> RewriteResult<Pair<String, A>, ?> fix(final NamedType<A> type, final RewriteResult<A, B> instance) {
            if (instance.view().isNop()) {
                return RewriteResult.nop(type);
            }
            return opticView(type, instance, wrapOptic(type.name, TypedOptic.adapter(instance.view().type(), instance.view().newType())));
        }

        @Override
        public RewriteResult<Pair<String, A>, ?> all(final TypeRewriteRule rule, final boolean recurse, final boolean checkIndex) {
            final RewriteResult<A, ?> elementView = element.rewriteOrNop(rule);
            return fix(this, elementView);
        }

        @Override
        public Optional<RewriteResult<Pair<String, A>, ?>> one(final TypeRewriteRule rule) {
            final Optional<RewriteResult<A, ?>> view = rule.rewrite(element);
            return view.map(instance -> fix(this, instance));
        }

        @Override
        public Type<?> updateMu(final RecursiveTypeFamily newFamily) {
            return DSL.named(name, element.updateMu(newFamily));
        }

        @Override
        public TypeTemplate buildTemplate() {
            return DSL.named(name, element.template());
        }

        @Override
        public Optional<TaggedChoice.TaggedChoiceType<?>> findChoiceType(final String name, final int index) {
            return element.findChoiceType(name, index);
        }

        @Override
        public Optional<Type<?>> findCheckedType(final int index) {
            return element.findCheckedType(index);
        }

        @Override
        protected Codec<Pair<String, A>> buildCodec() {
            return new Codec<Pair<String, A>>() {
                @Override
                public <T> DataResult<Pair<Pair<String, A>, T>> decode(final DynamicOps<T> ops, final T input) {
                    return element.codec().decode(ops, input).map(vo -> vo.mapFirst(v -> Pair.of(name, v))).setLifecycle(Lifecycle.experimental());
                }

                @Override
                public <T> DataResult<T> encode(final Pair<String, A> input, final DynamicOps<T> ops, final T prefix) {
                    if (!Objects.equals(input.getFirst(), name)) {
                        return DataResult.error(() -> "Named type name doesn't match: expected: " + name + ", got: " + input.getFirst(), prefix);
                    }
                    return element.codec().encode(input.getSecond(), ops, prefix).setLifecycle(Lifecycle.experimental());
                }
            };
        }

        @Override
        public String toString() {
            return "NamedType[\"" + name + "\", " + element + "]";
        }

        public String name() {
            return name;
        }

        public Type<A> element() {
            return element;
        }

        @Override
        public boolean equals(final Object obj, final boolean ignoreRecursionPoints, final boolean checkIndex) {
            if (this == obj) {
                return true;
            }
            if (!(obj instanceof NamedType<?>)) {
                return false;
            }
            final NamedType<?> other = (NamedType<?>) obj;
            return Objects.equals(name, other.name) && element.equals(other.element, ignoreRecursionPoints, checkIndex);
        }

        @Override
        public int hashCode() {
            int result = name.hashCode();
            result = 31 * result + element.hashCode();
            return result;
        }

        @Override
        public Optional<Type<?>> findFieldTypeOpt(final String name) {
            return element.findFieldTypeOpt(name);
        }

        @Override
        public Optional<Pair<String, A>> point(final DynamicOps<?> ops) {
            return element.point(ops).map(value -> Pair.of(name, value));
        }

        @Override
        public <FT, FR> Either<TypedOptic<Pair<String, A>, ?, FT, FR>, FieldNotFoundException> findTypeInChildren(final Type<FT> type, final Type<FR> resultType, final TypeMatcher<FT, FR> matcher, final boolean recurse) {
            return element.findType(type, resultType, matcher, recurse).mapLeft(o -> wrapOptic(name, o));
        }

        protected static <A, B, FT, FR> TypedOptic<Pair<String, A>, Pair<String, B>, FT, FR> wrapOptic(final String name, final TypedOptic<A, B, FT, FR> optic) {
            return new TypedOptic<>(
                Cartesian.Mu.TYPE_TOKEN,
                DSL.named(name, optic.sType()),
                DSL.named(name, optic.tType()),
                optic.sType(),
                optic.tType(),
                Optics.proj2()
            ).compose(optic);
        }
    }
}

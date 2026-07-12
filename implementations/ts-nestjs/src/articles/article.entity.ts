import { Column, Entity, PrimaryGeneratedColumn } from 'typeorm';

/**
 * Postgres `numeric` columns are returned as strings by the driver; this
 * transformer converts them back to JS numbers on read.
 */
const numericTransformer = {
  to: (value: number): number => value,
  from: (value: string | number): number =>
    typeof value === 'string' ? parseFloat(value) : value,
};

/**
 * Article/product entity.
 *
 * `version` is a plain integer optimistic-concurrency token that starts at 1
 * and is incremented on every update; it is surfaced to clients as
 * `row_version`. A PUT whose `row_version` no longer matches the current value
 * is rejected with 409 (see {@link ArticlesService.update}).
 */
@Entity('articles')
export class Article {
  @PrimaryGeneratedColumn({ name: 'article_id' })
  articleId!: number;

  @Column({ type: 'varchar', length: 64 })
  name!: string;

  @Column({ type: 'varchar', length: 2048 })
  description!: string;

  @Column({ type: 'varchar', length: 64, nullable: true })
  category!: string | null;

  @Column({
    type: 'numeric',
    precision: 18,
    scale: 2,
    transformer: numericTransformer,
  })
  price!: number;

  @Column({ type: 'varchar', length: 3, nullable: true })
  currency!: string | null;

  @Column({ type: 'int', default: 1 })
  version!: number;
}

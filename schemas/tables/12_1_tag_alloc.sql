-- Table: public.tag_alloc

-- DROP TABLE IF EXISTS public.tag_alloc;

CREATE TABLE IF NOT EXISTS public.tag_alloc
(
    item_id bigint NOT NULL,
    tag_id bigint NOT NULL,
    CONSTRAINT fk_item_id FOREIGN KEY (item_id)
        REFERENCES public.item (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT fk_tag_id FOREIGN KEY (tag_id)
        REFERENCES public.tag (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.tag_alloc
    OWNER to postgres;

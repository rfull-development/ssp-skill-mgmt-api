-- Table: public.alias

-- DROP TABLE IF EXISTS public.alias;

CREATE TABLE IF NOT EXISTS public.alias
(
    item_id bigint NOT NULL,
    alias_item_id bigint NOT NULL,
    CONSTRAINT fk_alias_item_id FOREIGN KEY (alias_item_id)
        REFERENCES public.item (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT fk_item_id FOREIGN KEY (item_id)
        REFERENCES public.item (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.alias
    OWNER to postgres;

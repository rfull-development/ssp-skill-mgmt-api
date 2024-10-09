-- Table: public.link_seg

-- DROP TABLE IF EXISTS public.link_seg;

CREATE TABLE IF NOT EXISTS public.link_seg
(
    item_id bigint NOT NULL,
    version integer NOT NULL DEFAULT 1,
    CONSTRAINT uq_link_seg_item_id UNIQUE (item_id),
    CONSTRAINT fk_item_id FOREIGN KEY (item_id)
        REFERENCES public.item (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.link_seg
    OWNER to postgres;

-- Table: public.link

-- DROP TABLE IF EXISTS public.link;

CREATE TABLE IF NOT EXISTS public.link
(
    item_id bigint NOT NULL,
    title character varying(64) COLLATE pg_catalog."default" NOT NULL,
    url text COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT fk_item_id FOREIGN KEY (item_id)
        REFERENCES public.item (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.link
    OWNER to postgres;

-- Table: public.item

-- DROP TABLE IF EXISTS public.item;

CREATE TABLE IF NOT EXISTS public.item
(
    id bigint NOT NULL DEFAULT nextval('seq_item_id'::regclass),
    guid uuid NOT NULL DEFAULT gen_random_uuid(),
    version integer NOT NULL DEFAULT 1,
    name character varying(128) COLLATE pg_catalog."default" NOT NULL,
    description text COLLATE pg_catalog."default",
    CONSTRAINT pk_item PRIMARY KEY (id),
    CONSTRAINT uq_item_guid UNIQUE (guid),
    CONSTRAINT uq_item_name UNIQUE (name)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.item
    OWNER to postgres;

-- Table: public.tag

-- DROP TABLE IF EXISTS public.tag;

CREATE TABLE IF NOT EXISTS public.tag
(
    id bigint NOT NULL DEFAULT nextval('seq_tag_id'::regclass),
    version integer NOT NULL DEFAULT 1,
    name character varying(128) COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT pk_tag PRIMARY KEY (id),
    CONSTRAINT uq_tag_name UNIQUE (name)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.tag
    OWNER to postgres;

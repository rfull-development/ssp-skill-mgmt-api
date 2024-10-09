-- View: public.list

-- DROP VIEW public.list;

CREATE OR REPLACE VIEW public.list
 AS
 SELECT id,
    guid,
    name
   FROM item i
  ORDER BY id;

ALTER TABLE public.list
    OWNER TO postgres;


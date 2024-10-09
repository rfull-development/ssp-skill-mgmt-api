-- View: public.tag_list

-- DROP VIEW public.tag_list;

CREATE OR REPLACE VIEW public.tag_list
 AS
 SELECT id,
    name
   FROM tag t
  ORDER BY id;

ALTER TABLE public.tag_list
    OWNER TO postgres;


-- View: public.tag_alloc_list

-- DROP VIEW public.tag_alloc_list;

CREATE OR REPLACE VIEW public.tag_alloc_list
 AS
 SELECT ta.item_id,
    t.id,
    t.name
   FROM tag_alloc ta
     LEFT JOIN tag t ON ta.tag_id = t.id
  ORDER BY ta.item_id, t.id;

ALTER TABLE public.tag_alloc_list
    OWNER TO postgres;


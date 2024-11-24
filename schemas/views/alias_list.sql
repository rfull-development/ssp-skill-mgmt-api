-- View: public.alias_list

-- DROP VIEW public.alias_list;

CREATE OR REPLACE VIEW public.alias_list
 AS
 SELECT a.item_id,
    i.guid,
    i.name
   FROM alias a
     LEFT JOIN item i ON a.alias_item_id = i.id
  ORDER BY a.alias_item_id;

ALTER TABLE public.alias_list
    OWNER TO postgres;


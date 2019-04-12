
CREATE TABLE public.test_data (
	id serial NOT NULL,
	"data" float8 NOT NULL,
	CONSTRAINT test_data_pkey PRIMARY KEY (id)
);


CREATE OR REPLACE FUNCTION public.get_data()
 RETURNS SETOF double precision
 LANGUAGE plpgsql
 STABLE
AS $function$
    BEGIN
      RETURN QUERY
      SELECT data
      FROM public.test_data;
    END
    $function$
;


select count(*) FROM public.test_data


select * from public.get_data();